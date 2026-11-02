# MD. Abu Saeed — Portfolio (ASP.NET Core + SQL Server)

A portfolio site with:
- A private admin panel (password-only login) to manage your GitHub / LinkedIn /
  Website / Facebook Page / Facebook Group / Instagram / YouTube / Gmail links
- CV upload/download
- Password recovery by email (a 6-digit code sent to your Gmail)
- A "Hire Me" project-inquiry form (floating "Let's talk" button + nav link)
  that emails submissions straight to your Gmail
- MS SQL Server as the database

## Stack

- ASP.NET Core 8 Web API (backend)
- Entity Framework Core + **SQL Server**
- JWT authentication (password only, no username)
- MailKit for sending email via Gmail SMTP
- Plain HTML/CSS/JS frontend, served by the same app
- Three.js (loaded from CDN, no build step) for the 3D scenes

---

## Step 1 — SQL Server setup

You said SQL Server is already installed on your PC, so:

1. Open **SQL Server Management Studio (SSMS)** (or Azure Data Studio) and
   connect to your local instance to confirm its name. Common cases:
   - Full SQL Server, default instance → server name is just `.` or `localhost`
   - SQL Server Express → server name is usually `localhost\SQLEXPRESS`
   - A named instance → `localhost\YourInstanceName`

2. You do **not** need to manually create the `PortfolioDb` database — the
   app creates it automatically on first run via EF Core migrations, as long
   as the connection string points at a server you can reach and your
   Windows account (or the account running Visual Studio) has permission to
   create databases there.

3. Open `PortfolioApp/appsettings.Development.json` and adjust the server
   name if yours isn't `localhost\SQLEXPRESS`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=PortfolioDb;Trusted_Connection=True;TrustServerCertificate=True;"
   }
   ```
   (Note the double backslash `\\` — that's how JSON escapes a single `\`.)

   If you use SQL Server auth (username/password) instead of Windows auth,
   use this form instead:
   ```
   Server=localhost;Database=PortfolioDb;User Id=sa;Password=YourPassword;TrustServerCertificate=True;
   ```

---

## Step 2 — Gmail App Password (for sending email)

Gmail will not accept your normal Gmail password from an app like this — you
need a separate **App Password**. This requires 2-Step Verification to be
turned on for `deeasabu@gmail.com` first.

1. Go to **myaccount.google.com/security**
2. Turn on **2-Step Verification** if it isn't already on
3. Go to **myaccount.google.com/apppasswords**
4. Under "App name", type something like `Portfolio Site` and click **Create**
5. Google shows a **16-character password** (like `abcd efgh ijkl mnop`) —
   copy it (remove the spaces when you paste it)

6. Open `PortfolioApp/appsettings.Development.json` and paste it in:
   ```json
   "Smtp": {
     "FromEmail": "deeasabu@gmail.com",
     "AppPassword": "paste-the-16-character-app-password-here"
   }
   ```

This app password is only used to send email as `deeasabu@gmail.com` — it
cannot be used to log into the Gmail account itself.

---

## Step 3 — First run (Visual Studio)

1. Open `PortfolioApp.sln` in Visual Studio, let NuGet packages restore.

2. Confirm `PortfolioApp/appsettings.Development.json` has:
   - Your SQL Server connection string (Step 1)
   - Your Gmail App Password (Step 2)
   - A real `Jwt:Key` (any long random string, 32+ characters)
   - `AdminSetup:InitialPassword` — your real admin login password
   - `AdminSetup:RecoveryEmail` — already set to `deeasabu@gmail.com`

3. Press **F5**. On first run the app will:
   - Create the `PortfolioDb` database and all tables on your SQL Server
   - Create your admin account (password hashed, never stored in plain text)
   - Open your browser to the homepage

4. Go to `/admin.html` (or click **Admin login** in the footer) and log in.

5. **After your first successful login**, you can clear
   `AdminSetup:InitialPassword` back to `""` — the account already exists in
   the database, so it's no longer needed and shouldn't be left sitting in a
   config file long-term.

---

## Using the admin panel

From `/admin.html`, after logging in:

- **Links** — add/edit/delete GitHub, LinkedIn, Website, Facebook Page,
  Facebook Group, Instagram, YouTube, Gmail, or repository links. These show
  up when a visitor clicks your profile photo, and social ones also appear
  in the Contact section.
- **Profile photo** — upload a JPG/PNG/WebP (max 5 MB). It replaces the hero
  photo immediately. "Use default" goes back to the built-in photo.
- **Homepage text** — change the headline, intro paragraph and About text.
  Leave a box empty to keep the built-in text.
- **Skills** — add, edit, reorder and delete skills. Each one appears as a card
  on the homepage and as a detail card on the skills page.
- **Projects** — add projects with description, tech stack (comma separated),
  a source-code (GitHub) link and a live-demo link. They appear in the
  Projects section.
- **CV** — upload a PDF (max 10 MB); it replaces the old one immediately.
- **Recovery email** — where "forgot password" codes get sent (defaults to
  `deeasabu@gmail.com`).
- **Change password** — update your password while logged in.

### Forgot password (if you're logged out and can't remember it)

On `/admin.html`, click **Forgot password?** then **Send reset code**, then
check `deeasabu@gmail.com` for a 6-digit code and enter it with a new
password. The code expires after 15 minutes.

---

## 3D scenes (Three.js)

- **Hero background:** floating wireframe shapes (accent-orange and ink) drift
  around and behind your portrait, and shift slightly as the mouse moves across
  the hero. They are kept off the headline area so the text stays easy to read.
- **Above the footer:** an interactive particle wave that rises near the cursor.
- Code is in `wwwroot/js/scenes.js`. The colours are the constants at the top.
- Animations pause when scrolled off-screen and respect the visitor's
  "reduce motion" setting. Three.js loads from cdnjs.cloudflare.com, so the
  visitor needs internet.

## "Let's talk" floating button

Opens a small popup with WhatsApp, Email and Phone (no form). To change the
numbers, edit the `talk-popup` block near the bottom of `wwwroot/index.html`.

## The "Hire Me" form

- The **Hire Me** link in the top nav opens this form.
- Visitors fill in: name, email, project type, budget (optional), project
  details, and whether they want a sample/demo first.
- On submit, this is emailed directly to your recovery email address
  (`deeasabu@gmail.com`) — nothing is stored in the database.

---

## Where everything lives

- `wwwroot/index.html` — public homepage (light theme, fixed-feel portrait,
  floating "Let's talk" button, Hire Me modal, social links)
- `wwwroot/skills.html` — skills detail page (loads skills from the database)
- `wwwroot/js/scenes.js` — the Three.js 3D scenes
- `wwwroot/admin.html` — private admin login + dashboard + password recovery
- `wwwroot/uploads/cv-current.pdf` — your uploaded CV (created after first upload)
- `Controllers/` — `AuthController` (login/password/recovery),
  `LinksController`, `ProfileController` (CV), `HireController` (project inquiries),
  `SkillsController`, `ProjectsController`, `SiteController` (photo + homepage text)
- `Services/EmailService.cs` — Gmail SMTP sending (used by both password
  recovery and the Hire Me form)
- `Models/`, `Data/` — database models and EF Core context
- `Migrations/` — SQL Server schema

---

## Deploying this publicly later

- Any host that supports ASP.NET Core + SQL Server: Azure App Service +
  Azure SQL Database (both have free/cheap tiers), or a VPS.
- Before deploying, move `Jwt:Key`, `Smtp:AppPassword`, and the SQL Server
  connection string into environment variables on the host rather than
  committing real secrets into `appsettings.json`.
- If you deploy the database separately (e.g. Azure SQL), update
  `ConnectionStrings:DefaultConnection` to point at that server instead of
  `localhost`.

---

## Security notes

- The admin password is hashed with BCrypt — the plain password is never
  stored anywhere in the database.
- Password reset codes are also hashed before being stored, expire after 15
  minutes, and can only be used once.
- The forgot-password endpoint always returns the same generic message,
  whether or not an account/email is configured, so it can't be used to
  guess whether this admin panel exists.
- Only login, forgot-password, reset-password, and hire are public; every
  other write endpoint requires a valid login token.
- CV uploads are restricted to PDF files, 10 MB max, and always saved under
  a fixed file name — visitor-supplied file names never touch the file path.

---

## Updating an existing database

If you already ran the previous version, just run the new one. On startup the
app applies the new migration (`AddContentTables`) automatically. It adds the
Skills, Projects and photo/text columns and keeps your admin password, links and
CV exactly as they are. You do not need to delete anything.

Starter skills and projects are added once so the page is not empty. Edit or
delete them in the admin panel.
