const btnOpenCategoryManagement = document.getElementById('btn-open-category-management');
const btnCloseCategoryModal = document.getElementById("modal-category-close");
const btnCategoryAddRow = document.getElementById("btn-category-add-row");
const btnCategoryEditRow = document.getElementById("btn-category-edit-row");
const btnCategoryDeleteRow = document.getElementById("btn-category-delete-row");
const modalCategoryDetails = document.getElementById("modal-category-details");
const btnCloseCategoryDetails = document.getElementById("category-details-close");
const txtCategoryDescription = document.getElementById("category-details-description");
const txtCategoryIdentifiers = document.getElementById("category-details-identifiers");
const radioCategoryType = document.querySelectorAll("input[name='category-type']");
const btnSaveCategoryDetails = document.getElementById("category-details-btn-save");
const hiddenCategoryId = document.getElementById("category-details-id");
const spanCategoryDetailsTitle = document.getElementById("category-details-title");

btnOpenCategoryManagement?.addEventListener('click', openModalCategoryManagement);
btnCloseCategoryModal?.addEventListener("click", closeModalCategoryManagement);
btnCategoryAddRow?.addEventListener("click", addCategory);
btnCategoryEditRow?.addEventListener("click", editCategory);
btnCategoryDeleteRow?.addEventListener("click", deleteCategory);
btnCloseCategoryDetails?.addEventListener("click", closeModalCategoryDetails);
btnSaveCategoryDetails?.addEventListener("click", saveCategoryDetail);

async function openModalCategoryManagement() {
    const modal = document.getElementById("modal-category-management");

    if (!modal) return;

    modal.style.display = "flex";

    await populateCategoryGrid();

    await setupCategoryGridSelection();
}

function closeModalCategoryManagement() {
    const modal = document.getElementById("modal-category-management");

    if (modal) {
        modal.style.display = "none";
    }
}    

async function populateCategoryGrid() {
    const gridBody = document.getElementById("body-category-grid");

    if (!gridBody) return;

    gridBody.innerHTML = "";

    const categorias = await getFromUrl('/api/categories');

    if (!categorias || categorias.length === 0) return;

    categorias.forEach(categoria => {
        const row = document.createElement("div");

        row.className = "grid-row";

        row.setAttribute("data-id", categoria.id);

        row.style.gridTemplateColumns = "1fr";

        const cell = document.createElement("div");

        cell.textContent = categoria.description;

        row.appendChild(cell);

        gridBody.appendChild(row);
    });
}

async function setupCategoryGridSelection() {
    const gridBody = document.getElementById("body-category-grid");

    if (!gridBody) return;

    let selectedRow = null;

    gridBody.addEventListener("click", (event) => {
        const row = event.target.closest(".grid-row");

        if (!row || !gridBody.contains(row)) return;

        clearSelection();
        selectRow(row);
    });

    gridBody.addEventListener("dblclick", async (event) => {
        const row = event.target.closest(".grid-row");

        if (!row || !gridBody.contains(row)) return;

        clearSelection();
        selectRow(row);

        await editCategory();
    });

    function clearSelection() {
        gridBody.querySelectorAll(".category-grid-row-selected").forEach(el => {
            el.classList.remove("category-grid-row-selected");
        });

        selectedRow = null;
    }

    function selectRow(row) {
        if (!row) return;

        row.classList.add("category-grid-row-selected");

        selectedRow = row;

        selectedRow.scrollIntoView({ block: "nearest", behavior: "smooth" });
    }


    document.addEventListener("keydown", (event) => {
        if (!selectedRow) return;

        const rows = Array.from(gridBody.querySelectorAll(".grid-row"));
        const currentIndex = rows.indexOf(selectedRow);

        if (event.key === "ArrowDown") {
            event.preventDefault();

            if (currentIndex < rows.length - 1) {
                clearSelection();

                selectRow(rows[currentIndex + 1]);
            }
        } else if (event.key === "ArrowUp") {
            event.preventDefault();

            if (currentIndex > 0) {
                clearSelection();

                selectRow(rows[currentIndex - 1]);
            }
        }
    });
}

async function addCategory() {
   await openModalCategoryDetails(null);
}

async function editCategory() {
    const id = await getCategoryGridId()

    if (id) await openModalCategoryDetails(id);
}

async function deleteCategory() {
    const id = await getCategoryGridId()

    if (id) {
        const confirm = await showMessageBox('Tem certeza que deseja excluir o registro selecionado?', true);

        if (!confirm) return;

        const result = await deleteFromUrl(`/api/categories/Delete/${id}`);

        if (result.success) {            
            await populateCategoryGrid();
        }
    }
}

async function getCategoryGridId() {
    const selected = document.querySelector(".category-grid-row-selected");

    if (!selected) {
        await showMessageBox("Selecione um registro válido.", false);
        
        return null;
    }

    const id = selected.getAttribute("data-id");

    return id ? parseInt(id, 10) : null;
}

async function openModalCategoryDetails(id) {  
    if (!modalCategoryDetails) return;

    hiddenCategoryId.value = id ?? "";

    spanCategoryDetailsTitle.textContent = id ? "Editar categoria" : "Criar nova categoria";

    if (id) {
        const dto = await getFromUrl(`/api/categories/${id}`);

        if (!dto) return;

        txtCategoryDescription.value = dto.description ?? '';
        txtCategoryIdentifiers.value = dto.identifiers ?? '';

        if (dto.radioName === 'rdbFixed') {
            document.querySelector('input[name="category-type"][value="fixed"]').checked = true;
        } else if (dto.radioName === 'rdbVariable') {
            document.querySelector('input[name="category-type"][value="variable"]').checked = true;
        }
    }

    modalCategoryDetails.style.display = "flex";
}

function closeModalCategoryDetails() {
    if (!modalCategoryDetails) return;

    radioCategoryType.forEach(radio => {
        radio.checked = false;
    });

    txtCategoryDescription.value = "";
    txtCategoryIdentifiers.value = "";
    hiddenCategoryId.value = "";

    modalCategoryDetails.style.display = "none";
}

async function saveCategoryDetail() {
    const dto = {
        Id: getCategoryId(),
        Description: getCategoryDescription(),
        RadioName: getCategoryRadioName(),
        Identifiers: getCategoryIdentifiers()
    };

    const result = await postFromBody('/api/categories/save', dto);

    if (result.success) {
        await populateCategoryGrid();

        closeModalCategoryDetails();
    }
}

function getCategoryId() {
    const raw = hiddenCategoryId.value;
    const parsed = parseInt(raw);

    return isNaN(parsed) ? null : parsed;
}

function getCategoryDescription() {
    const value = txtCategoryDescription.value.trim();

    return value.length > 0 ? value : null;
}

function getCategoryIdentifiers() {
    const value = txtCategoryIdentifiers.value.trim();

    return value.length > 0 ? value : null;
}

function getCategoryRadioName() {
    const selected = Array.from(radioCategoryType).find(r => r.checked);

    if (!selected) return null;

    return selected.value === 'fixed' ? 'rdbFixed' : selected.value === 'variable' ? 'rdbVariable' :  null;
}