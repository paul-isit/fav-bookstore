# Website folder

This folder is intentionally plain frontend code only:

- `index.html` - catalogue homepage with cart preview popup
- `cart.html` - full cart and checkout page
- `login.html` - login page
- `signup.html` - signup page
- `styles.css`
- `script.js`
- `account.js`

It does not duplicate the original C# `Controllers`, `Models`, `Services`, or `Views` folders.

The JavaScript tries to read the original book data from:

```text
../Infrastructure/data/books.json
```

Run from the `fav-bookstore-main` folder:

```bash
python3 -m http.server 5500
```

Open:

```text
http://localhost:5500/website/index.html
```
<!-- TO RUN
RUN FROM fav-bookstore-main: python3 -m http.server 5500
OPEN: http://localhost:5500/website/index.html -->