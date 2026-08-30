# Screenshots

| File | What it shows | Status |
| --- | --- | --- |
| `screenshot-dashboard.png` | The summary page at desktop width | done |
| `screenshot-transactions.png` | An account statement at desktop width | done |
| `screenshot-import.png` | The import screen | done; worth replacing with one taken after pasting, so the validated preview shows |
| `screenshot-mobile.png` | The summary page at 375px | to capture |

Capture the browser viewport only: no address bar, no taskbar, and a window
with no extensions on show. Run against the demo data from `database/seed.sql`
so that no real financial record ends up in a public image.

## The import preview

The point of this one is the validated preview, so paste before capturing.
Rows with tab-separated columns, including two bad ones so the errors show up
next to the accepted rows:

```
05/09/2026	05/09/2026	SUPERMERCADO DEMO FILIAL 04	-312,45
06/09/2026	10/09/2026	PAG BOLETO ENERGIA ELETRICA	-287,63
DATA_ERRADA	08/09/2026	PADARIA CENTRAL	-45,90
09/09/2026	09/09/2026	POSTO COMBUSTIVEL AVENIDA	ABC
10/09/2026	10/09/2026	HORTIFRUTI BAIRRO NOVO	-74,30
```

The first two match category keywords and come back categorised, the third
carries an invalid date and the fourth an invalid amount, and the last is left
uncategorised.

## The mobile view

Open the summary page, then the browser device toolbar at 375px wide.
