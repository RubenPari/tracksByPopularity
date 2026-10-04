# Documento dei Requisiti di Sistema - TracksByPopularity

## 1. Introduzione e Obiettivi del Progetto

### 1.1 Scopo del Progetto
**TracksByPopularity** è una piattaforma web full-stack composta da un frontend Single Page Application (SPA) in **React con TypeScript** e un backend ad altissime prestazioni sviluppato con **Bun** e **ElysiaJS** (https://elysiajs.com/), con persistenza relazionale su **PostgreSQL** e caching distribuito su **Redis**, progettata per integrarsi a fondo con l'ecosistema **Spotify**.

Il sistema permette agli utenti di analizzare la propria libreria musicale di brani salvati, categorizzarli automaticamente in playlist dedicate in base all'indice di popolarità fornito da Spotify (valore intero normalizzato tra 0 e 100) e generare/gestire playlist tematiche focalizzate sui propri artisti preferiti.

Oltre alla categorizzazione intelligente, la piattaforma offre funzionalità avanzate di:
- **Sicurezza e Disaster Recovery (Snapshot System)**: Creazione automatica di snapshot di sicurezza prima di qualsiasi operazione distruttiva o di modifica massiva sulle playlist, con consultazione della cronologia e capacità di ripristino atomico istantaneo (*undo/restore*).
- **Doppia Modalità di Accesso**: Supporto sia per sessioni dirette tramite Spotify OAuth 2.0, sia per un sistema di account utente nativo (Email/Password con hashing sicuro via `Bun.password` e token JWT).
- **Interfaccia Utente Reattiva e Type-Safe**: Frontend moderno in React con TypeScript (Vite), con sincronizzazione dati e validazione end-to-end garantita dall'integrazione con ElysiaJS ed Eden Treaty.
- **Architettura Resiliente e Performante**: Runtime Bun con I/O asincrono nativo ultra-rapido, livello di caching distribuito con Redis, compressione GZip e policy di retry per minimizzare il consumo della quota di rate-limiting delle API di Spotify.

---

## 2. Architettura di Sistema e Stack Tecnologico

Il progetto adotta un'architettura modulare a livelli (Layered / Clean Architecture) guidata dai principi del Domain-Driven Design (DDD) e supportata dal paradigma a plugin e composizione proprio di ElysiaJS e TypeScript.

```mermaid
graph TD
    Client["Frontend SPA (React + TypeScript / Vite)"] -->|"HTTPS / Eden Treaty / REST"| Presentation["Presentation Layer (ElysiaJS Routes, Hooks, Validation TypeBox, Middlewares)"]
    Presentation --> AppService["Application Layer (Services: Track, Playlist, Backup, Auth, Spotify)"]
    AppService --> Domain["Domain Layer (Entities, DTOs, Schemi di Dominio, Interfacce)"]
    AppService --> Infra["Infrastructure Layer (Drizzle ORM, PostgreSQL, Redis, Spotify API, Mailer)"]
    Infra --> Domain
    Presentation --> Infra
    
    subgraph Storage
        Postgres[(PostgreSQL 16+)]
        Redis[(Redis Cache)]
    end
    
    subgraph External
        SpotifyAPI["Spotify Web API"]
        Mailtrap["Mailtrap (Email Service)"]
    end
    
    Infra --> Postgres
    Infra --> Redis
    Infra --> SpotifyAPI
    Infra --> Mailtrap
```

### 2.1 Stack Tecnologico

#### Frontend
- **Framework & Libreria**: **React 19** (o 18+) con **TypeScript** per garantire massima affidabilità e type safety nel codice UI.
- **Build Tool & Bundler**: **Vite** per tempi di avvio istantanei, HMR (Hot Module Replacement) ultra-rapido e build di produzione ottimizzate.
- **API Client & Type-Safety**: **Eden Treaty** (client nativo per ElysiaJS che condivide i tipi TypeScript dell'API senza necessità di code generation) o TanStack Query (React Query) per gestione di caching client, stato asincrono e invalidazioni.
- **Styling & UI**: **Tailwind CSS** con libreria di componenti headless (es. Radix UI / Lucide React) per un'interfaccia moderna, responsive e in tema Spotify (Dark Mode nativa).
- **Routing**: **React Router** con rotte protette (Auth Guards) per sessioni autenticate e gestione redirect OAuth.

#### Backend
- **Runtime**: **Bun** (v1.1+ / v1.2+), runtime JavaScript/TypeScript ultra-performante basato su JavaScriptCore, con supporto nativo a TypeScript, bundling, test runner e API di sistema ad alta velocità.
- **Web Framework**: **ElysiaJS** (v1.x+, https://elysiajs.com/), framework HTTP ergonomico progettato specificamente per Bun, caratterizzato da:
  - Routing ad alte prestazioni basato su compilazione JIT e analisi statica.
  - Validazione runtime e serializzazione dichiarativa tramite **TypeBox** (`t`).
  - End-to-end type safety garantita verso il client tramite **Eden**.
  - Sistema a plugin componibili (`@elysiajs/cors`, `@elysiajs/jwt`, `@elysiajs/cron`, `@elysiajs/swagger`).
- **Data Persistence & ORM**: **PostgreSQL 16+** gestito tramite **Drizzle ORM** (TypeScript-first, zero-overhead, schema as code) con **Drizzle Kit** per la gestione e l'applicazione automatica delle migrazioni SQL all'avvio.
- **Distributed Caching**: **Redis** (client `ioredis` ottimizzato per Bun), corredato da compressione payload con GZip (`zlib` nativo) e wrapper con retry logic esponenziale per prevenire rate-limit e gestire disconnessioni transitorie.
- **Sicurezza & Autenticazione**:
  - Hashing credenziali tramite `Bun.password` (algoritmo sicuro bcrypt/argon2id nativo ad alta velocità).
  - Autenticazione JWT tramite `@elysiajs/jwt` con cookie HTTP-Only (`access_token`, `SameSite=Strict`, `Secure`).
- **Servizi Transazionali & Terze Parti**:
  - Integrazione Spotify Web API tramite client TypeScript nativo basato su `fetch` ottimizzato di Bun con gestione automatica del refresh dei token OAuth.
  - Mailtrap / Nodemailer per l'invio di email transazionali (attivazione account e reset credenziali).
- **Documentazione API**: Swagger/OpenAPI integrato automaticamente via `@elysiajs/swagger` accessibile su `/swagger`.
- **Logging & Monitoraggio**: Logger strutturato JSON (Pino / log middleware) con tracciamento di request ID, tempo di risposta e rotazione file di log.
- **Containerizzazione & Hosting**: Dockerfile multistage basato sull'immagine ufficiale `oven/bun` con supporto per deployment containerizzato (es. Fly.io, Docker Compose, VPS).

---

## 3. Requisiti Funzionali (Functional Requirements)

### RF-01: Gestione Account Locale e Autenticazione JWT
- **RF-01.1 Registrazione Utente**: L'utente può registrarsi fornendo email valida e password. La password viene sottoposta ad hashing sicuro tramite `Bun.password` prima del salvataggio nel database PostgreSQL.
- **RF-01.2 Verifica Email**: Alla registrazione viene generato un token crittografico univoco salvato su PostgreSQL e inviato via email tramite Mailtrap. L'account è abilitato al login solo previa verifica positiva tramite endpoint o link dedicato.
- **RF-01.3 Login con JWT**: L'utente registrato può autenticarsi ricevendo un token JWT firmato (validità 7 giorni). Il token viene trasmesso sia nel corpo della risposta sia memorizzato in un cookie sicuro `HttpOnly` (`access_token`, `SameSite=Strict`).
- **RF-01.4 Recupero e Reset Password**: Flusso di recupero password con generazione di token monouso a scadenza temporale, notifica email e validazione della nuova credenziale.
- **RF-01.5 Cambio Password Autenticato**: Gli utenti autenticati possono modificare la propria password fornendo la password attuale e quella nuova.
- **RF-01.6 Informazioni Profilo (`/me`)**: Accesso alle informazioni dell'account autenticato (id, email, stato verifica email, stato collegamento Spotify).
- **RF-01.7 Logout Locale**: Invalidazione e rimozione del cookie di sessione JWT.

### RF-02: Integrazione Spotify OAuth 2.0
- **RF-02.1 Flusso Authorization Code**: Generazione dell'URL di autorizzazione verso Spotify con gli scope necessari:
  - `user-read-email`, `user-read-private`, `user-library-read`, `user-library-modify`, `user-top-read`, `playlist-modify-private`, `playlist-modify-public`, `user-follow-read`.
- **RF-02.2 Scambio Codice e Salvataggio Token**: Ricezione del codice di autorizzazione nella callback OAuth, scambio con Access Token e Refresh Token, e archiviazione crittografata su Redis associata allo `SpotifyUserId`.
- **RF-02.3 Tracciamento Sessione Spotify**: Gestione del contesto utente tramite cookie `spotify_user_id` e supporto all'header HTTP `X-Spotify-User-Id` (prioritario rispetto al cookie) per scenari client-side/cross-origin tra React e backend ElysiaJS.
- **RF-02.4 Verifica Stato Autenticazione (`/auth/is-auth`)**: Validazione in tempo reale della sessione verificando la disponibilità e validità del token memorizzato in Redis.
- **RF-02.5 Logout Spotify**: Rimozione dei token da Redis e cancellazione del cookie associato.

### RF-03: Collegamento Account Spotify con Account Locale
- **RF-03.1 Link Spotify Account**: Un utente autenticato con account nativo può associare il proprio account Spotify completando il flusso OAuth e registrando il mapping nella tabella `spotify_links` di PostgreSQL.
- **RF-03.2 Status Collegamento**: Possibilità di verificare se l'account locale corrente è collegato a Spotify e con quale identificativo (`SpotifyUserId`).
- **RF-03.3 Unlink Spotify Account**: Scollegamento sicuro dell'account Spotify, con cancellazione del record su database e rimozione delle credenziali in cache Redis.

### RF-04: Organizzazione Brani per Indice di Popolarità
- **RF-04.1 Fasce di Popolarità Supportate**:
  - `less`: popolarità da **0 a 20**
  - `less-medium`: popolarità da **21 a 40**
  - `medium`: popolarità da **41 a 60**
  - `more-medium`: popolarità da **61 a 80**
  - `more`: popolarità da **81 a 100**
- **RF-04.2 Nomenclatura Playlist**: Creazione o riuso della playlist di destinazione secondo la convenzione `Popularity: {Min}-{Max}` (es. `Popularity: 41-60`).
- **RF-04.3 Procedura di Aggiornamento Sicuro**:
  1. Ricerca o creazione della playlist di riferimento sul profilo Spotify dell'utente.
  2. Creazione preventiva dello **snapshot** di backup della playlist esistente su PostgreSQL.
  3. Svuotamento integrale delle tracce correnti dalla playlist.
  4. Filtraggio delle tracce salvate della libreria utente appartenenti al range specificato.
  5. Inserimento a blocchi (*batching* fino a 100 tracce per richiesta, secondo i vincoli di Spotify).

### RF-05: Organizzazione Brani per Artista
- **RF-05.1 Elenco Artisti della Libreria (`/api/track/artists`)**: Estrazione degli artisti unici presenti nei brani salvati che l'utente segue attivamente, con aggregazione e ordinamento decrescente per numero di tracce possedute.
- **RF-05.2 Ripartizione per Artista su 3 Playlist**:
  - Fascia `less`: popolarità da **0 a 33** &rarr; playlist `{ArtistName} less`
  - Fascia `medium`: popolarità da **34 a 66** &rarr; playlist `{ArtistName} medium`
  - Fascia `more`: popolarità da **67 a 100** &rarr; playlist `{ArtistName} more`
- **RF-05.3 Workflow Atomico per Artista**: Backup preventivo di ciascuna delle tre playlist, svuotamento controllato e ripopolamento selettivo con i brani dell'artista corrispondenti a ciascuna fascia.

### RF-06: Backup e Ripristino Playlist (Snapshot System)
- **RF-06.1 Snapshot Automatico**: Cattura dello stato corrente di una playlist (metadati e lista ordinata di URI delle tracce) prima di ogni operazione di categorizzazione.
- **RF-06.2 Consultazione Snapshot (`/api/backup/list`)**: Recupero della lista storica degli snapshot generati per l'utente corrente, corredati di timestamp, nome playlist, tipo operazione e conteggio tracce.
- **RF-06.3 Ripristino Playlist (`/api/backup/restore/{snapshotId}`)**: Ripristino atomico dello stato salvato nello snapshot sulla playlist originale Spotify, con contestuale invalidazione della cache Redis delle playlist.
- **RF-06.4 Cancellazione Manuale Snapshot (`/api/backup/{snapshotId}`)**: Eliminazione manuale di uno specifico snapshot dal database PostgreSQL.

### RF-07: Gestione Cache e Playlist Utente
- **RF-07.1 Elenco Playlist (`/api/playlist/all`)**: Restituzione delle playlist possedute dall'utente con caching a 15 minuti su Redis.
- **RF-07.2 Refresh Forzato (`/api/playlist/refresh`)**: Invalidazione esplicita della cache Redis delle playlist e nuovo recupero aggiornato da Spotify.

### RF-08: Health Check & Monitoraggio
- **RF-08.1 Endpoint di Liveness (`/health`)**: Endpoint pubblico che risponde con codice HTTP 200 e payload `{ status: "healthy", timestamp: ... }` per health probe di Docker, Fly.io e monitor esterni.

### RF-09: Interfaccia Utente e Funzionalità Frontend (React TS)
- **RF-09.1 Dashboard Principale**:
  - Visualizzazione dello stato profilo utente, avatar Spotify e badge di connessione attiva.
  - Indicatori sintetici su totale brani salvati nella libreria e playlist gestite.
- **RF-09.2 Gestione Popolarità**:
  - Selettore interattivo delle 5 fasce di popolarità (`0-20`, `21-40`, `41-60`, `61-80`, `81-100`).
  - Anteprima dinamica dei brani che rientrano nella fascia selezionata prima di applicare le modifiche.
  - Pulsante di sincronizzazione con indicatore di caricamento (*loading spinner/progress bar*) e notifica di completamento (Toast).
- **RF-09.3 Gestione Artisti**:
  - Visualizzazione ad elenco o griglia degli artisti seguiti con ordinamento per popolarità o numero di tracce salvate.
  - Barra di ricerca con debounce per filtrare rapidamente gli artisti.
  - Azione one-click per avviare la ripartizione in 3 playlist (`less`, `medium`, `more`) con modale di conferma e avviso di snapshot preventivo.
- **RF-09.4 Centro Backup e Ripristino**:
  - Tabella cronologica degli snapshot con data/ora, playlist target, tipologia operazione e numero tracce salvate.
  - Azione di **Restore** con modale di conferma e spiegazione dell'effetto (ripristino esatto dello stato precedente).
  - Azione di eliminazione manuale dello snapshot con feedback immediato.
- **RF-09.5 Gestione Autenticazione & Notifiche Client**:
  - Form reattivi di Registrazione, Login, Richiesta Reset Password e Cambio Password con validazione lato client (React Hook Form / Zod).
  - Gestione flessibile della modalità di accesso (solo Spotify o con Account Locale collegato).
  - Sistema di notifiche Toast unificato per feedback su operazioni riuscite ed errori API.

---

## 4. Requisiti Non Funzionali (Non-Functional Requirements)

### RNF-01: Performance e Caching
- **RNF-01.1 Performance I/O con Bun ed ElysiaJS**: L'applicazione sfrutta l'engine JavaScriptCore nativo di Bun e le route JIT-compiled di ElysiaJS per garantire tempi di risposta sub-millisecondo per le route in-memory e overhead CPU minimo.
- **RNF-01.2 Cache Redis Multi-livello**:
  - Tracce della libreria utente (`tracks:{spotifyUserId}`)
  - Elenco playlist (`playlists:{spotifyUserId}`)
  - Artisti seguiti (`artists:{spotifyUserId}`)
  - Token OAuth (`spotify_token:{spotifyUserId}`)
- **RNF-01.3 Compressione Dati**: Tutti i payload salvati su Redis superiori alla soglia critica (es. 2KB) sono compressi tramite **GZip** nativo per ridurre la memoria RAM occupata e la latenza di rete.
- **RNF-01.4 Resilienza e Rate-Limiting**: Chiamate verso le API Spotify provviste di backoff esponenziale in caso di HTTP 429 (`Retry-After`) per rispettare i vincoli del provider e garantire continuità di servizio.
- **RNF-01.5 HTTP Response Caching & Headers**: Risposte fornite con header `Cache-Control` ed `ETag` ove opportuno per ottimizzare la fruizione dal frontend React.

### RNF-02: Sicurezza e Protezione dei Dati
- **RNF-02.1 Crittografia Password**: Password cifrate con `Bun.password.hash(pwd, { algorithm: "bcrypt", cost: 10 })` o `argon2id` con salt crittografico generato automaticamente.
- **RNF-02.2 Firma e Validità Token JWT**: Token generati con `@elysiajs/jwt` con firma HMAC-SHA256 basata su chiave simmetrica sicura (minimo 32 caratteri) e scadenza a 7 giorni.
- **RNF-02.3 Cookie Sicuri**: I cookie di sessione (`access_token`, `spotify_user_id`) devono avere obbligatoriamente i flag `HttpOnly`, `SameSite=Strict` (o `Lax` durante callback OAuth) e `Secure=true` in produzione.
- **RNF-02.4 Isolamento Dati e Autorizzazione**: Ogni utente può accedere esclusivamente ai propri snapshot, playlist e credenziali Spotify. Nessuna operazione può manipolare o interrogare dati appartenenti ad altri account.
- **RNF-02.5 Validazione Input Rigorosa**: Tutte le richieste HTTP in ingresso sono validate tramite schemi **TypeBox** (`t`) in ElysiaJS, rigettando payload non conformi con errore standardizzato 422/400 prima dell'esecuzione dei controller.
- **RNF-02.6 CORS Controllato**: Configurazione di `@elysiajs/cors` ristretta all'origine del frontend React specificata tramite `FRONTEND_ORIGIN` (es. `http://localhost:5173` in sviluppo).

### RNF-03: Affidabilità e Integrità
- **RNF-03.1 Pattern Snapshot-Before-Mutation**: Nessuna playlist può essere svuotata o riscritta su Spotify senza che sia stato preventivamente persistito uno snapshot coerente su PostgreSQL.
- **RNF-03.2 Gestione Globale delle Eccezioni**: Tutte le eccezioni non gestite vengono intercettate dall'hook `onError` di ElysiaJS, registrate nei log strutturati con stack trace e convertite nella risposta uniforme `ApiResponse.Fail(...)`.
- **RNF-03.3 Migrazioni Automatiche del Database**: Drizzle Kit applica in modo deterministico e automatico le migrazioni SQL su PostgreSQL all'avvio del container backend.

### RNF-04: Manutenibilità e Pulizia Automatica
- **RNF-04.1 Retention Snapshot (30 giorni)**: Un job pianificato tramite `@elysiajs/cron` si attiva ogni 24 ore ed elimina automaticamente da PostgreSQL gli snapshot più vecchi di 30 giorni e le tracce correlate (eliminazione a cascata).
- **RNF-04.2 Controllo e Pulizia Cache**: Job periodico (ogni 5 minuti) che monitora la salute della connessione Redis e pulisce chiavi orfane o invalidate.

### RNF-05: Logging e Observability
- **RNF-05.1 Logging Strutturato**: Tutti gli eventi applicativi e gli errori sono tracciati in formato JSON strutturato con metadati essenziali: `timestamp`, `level`, `requestId`, `path`, `method` ed `executionTimeMs`.
- **RNF-05.2 Rotazione Log**: File di log archiviati su base giornaliera con retention massima di 30 giorni per agevolare audit e debugging.

---

## 5. Modello Dati e Schema Database (PostgreSQL)

Il database adotta **PostgreSQL 16+** con tipi di dato nativi avanzati come `UUID`, `TIMESTAMPTZ`, `VARCHAR` e indici B-Tree ottimizzati.

```mermaid
erDiagram
    users ||--o| spotify_links : "possiede"
    users ||--o{ email_verification_tokens : "ha"
    users ||--o{ password_reset_tokens : "ha"
    users ||--o{ playlist_snapshots : "archivia (opzionale)"
    playlist_snapshots ||--|{ snapshot_tracks : "contiene"

    users {
        uuid id PK
        varchar email UK
        varchar password_hash
        boolean is_email_verified
        timestamptz created_at
        timestamptz updated_at
    }

    spotify_links {
        uuid id PK
        uuid user_id FK, UK
        varchar spotify_user_id UK
        text access_token
        text refresh_token
        timestamptz created_at
        timestamptz updated_at
    }

    email_verification_tokens {
        uuid id PK
        uuid user_id FK
        varchar token UK
        timestamptz created_at
    }

    password_reset_tokens {
        uuid id PK
        uuid user_id FK
        varchar token UK
        timestamptz created_at
    }

    playlist_snapshots {
        uuid id PK
        uuid user_id FK "nullable"
        varchar spotify_user_id
        varchar playlist_id
        varchar playlist_name
        varchar operation_type
        timestamptz created_at
    }

    snapshot_tracks {
        serial id PK
        uuid snapshot_id FK
        varchar track_uri
    }
```

### 5.1 Dettaglio Tabelle e Vincoli (Drizzle ORM / PostgreSQL)

- **`users`**:
  - `id`: `uuid` primario generato con `defaultRandom()`.
  - `email`: `varchar(255)`, non nullo, con vincolo di unicità `uniqueIndex`.
  - `password_hash`: `varchar(255)`, hash generato con `Bun.password`.
  - `is_email_verified`: `boolean`, default `false`.
  - `created_at` / `updated_at`: `timestamptz`, default `now()`.

- **`spotify_links`**:
  - `id`: `uuid` primario (`defaultRandom()`).
  - `user_id`: `uuid` referente `users.id` con vincolo di unicità (`unique`) e `onDelete: 'cascade'`.
  - `spotify_user_id`: `varchar(255)`, non nullo, univoco.
  - `access_token` / `refresh_token`: `text`, token crittografati per l'accesso offline.
  - `created_at` / `updated_at`: `timestamptz`.

- **`email_verification_tokens` & `password_reset_tokens`**:
  - `id`: `uuid` primario.
  - `user_id`: `uuid` referente `users.id` (`onDelete: 'cascade'`).
  - `token`: `varchar(255)`, univoco.
  - `created_at`: `timestamptz`.

- **`playlist_snapshots`**:
  - `id`: `uuid` primario (`defaultRandom()`).
  - `user_id`: `uuid` referente `users.id`, nullable (`onDelete: 'set null'`) per supportare sessioni dirette Spotify senza account locale registrato.
  - `spotify_user_id`: `varchar(255)`, indice per lookup veloci.
  - `playlist_id`: `varchar(255)`, identificativo Spotify della playlist.
  - `playlist_name`: `varchar(255)`.
  - `operation_type`: `varchar(50)` (es. `popularity_sort`, `artist_split`).
  - `created_at`: `timestamptz`, indice per query di retention ed eliminazione programmata.

- **`snapshot_tracks`**:
  - `id`: `serial` primario auto-incrementale.
  - `snapshot_id`: `uuid` referente `playlist_snapshots.id` con `onDelete: 'cascade'`.
  - `track_uri`: `varchar(255)` (es. `spotify:track:4cOdK2wGLETKBW3PvgPWqT`).

---

## 6. Specifiche e Contratti delle API

Tutti gli endpoint rispondono con una struttura JSON standardizzata `ApiResponse<T>`:

```json
{
  "success": true,
  "data": { ... },
  "message": "Operazione completata con successo",
  "error": null
}
```

In caso di errore:
```json
{
  "success": false,
  "data": null,
  "message": "Descrizione leggibile dell'errore",
  "error": "CODICE_O_DETTAGLIO_ERRORE"
}
```

### 6.1 Mappa Completa degli Endpoint

Grazie a **ElysiaJS** ed **Eden Treaty**, ogni endpoint espone contratti strettamente tipizzati consumabili direttamente dal frontend React con autocompletamento nativo in TypeScript.

| Modulo | Metodo | Endpoint | Auth Richiesta | Descrizione |
|---|---|---|---|---|
| **Health** | `GET` | `/health` | Nessuna | Verifica stato di funzionamento del servizio |
| **Documentazione** | `GET` | `/swagger` | Nessuna | Interfaccia interattiva Swagger UI (OpenAPI 3.0) |
| **Auth Spotify** | `GET` | `/auth/login` | Nessuna | Restituisce l'URL di redirect Spotify |
| **Auth Spotify** | `GET` | `/auth/callback` | Nessuna | Callback OAuth: scambio token e salvataggio su Redis |
| **Auth Spotify** | `GET` | `/auth/is-auth` | Cookie o Header | Verifica sessione Spotify attiva |
| **Auth Spotify** | `POST` | `/auth/logout` | Cookie o Header | Logout Spotify e rimozione credenziali da Redis |
| **Account** | `POST` | `/api/account/register` | Nessuna | Registrazione nuovo utente nativo |
| **Account** | `POST` | `/api/account/login` | Nessuna | Login utente (restituisce JWT e imposta cookie) |
| **Account** | `GET` | `/api/account/verify/:token`| Nessuna | Verifica email utente |
| **Account** | `POST` | `/api/account/forgot-password`| Nessuna | Richiesta link reset password |
| **Account** | `POST` | `/api/account/reset-password` | Nessuna | Impostazione nuova password tramite token |
| **Account** | `POST` | `/api/account/change-password`| JWT Bearer | Modifica password per utente autenticato |
| **Account** | `POST` | `/api/account/link-spotify` | JWT Bearer | Associazione credenziali Spotify ad account locale |
| **Account** | `GET` | `/api/account/me` | JWT Bearer | Informazioni profilo utente corrente |
| **Account** | `POST` | `/api/account/logout` | JWT Bearer | Invalida il cookie `access_token` |
| **Spotify Link**| `GET` | `/api/spotify/link-url` | JWT Bearer | Genera URL per il link Spotify autenticato |
| **Spotify Link**| `GET` | `/api/spotify/callback` | Nessuna | Callback OAuth di associazione account |
| **Spotify Link**| `GET` | `/api/spotify/status` | JWT Bearer | Verifica se l'account locale è collegato a Spotify |
| **Spotify Link**| `POST` | `/api/spotify/unlink` | JWT Bearer | Scollega l'account Spotify associato |
| **Tracce** | `POST` | `/api/track/popularity/:range`| Spotify Auth | Popola playlist per range di popolarità specificato |
| **Tracce** | `GET` | `/api/track/artists` | Spotify Auth | Lista artisti seguiti con conteggio brani salvati |
| **Tracce** | `POST` | `/api/track/artist` | Spotify Auth | Organizza i brani di un artista in 3 playlist |
| **Playlist** | `GET` | `/api/playlist/all` | Spotify Auth | Recupera tutte le playlist possedute (con cache 15m) |
| **Playlist** | `POST` | `/api/playlist/refresh` | Spotify Auth | Invalida la cache Redis e ricarica da Spotify |
| **Backup** | `GET` | `/api/backup/list` | Spotify Auth | Lista cronologica degli snapshot utente |
| **Backup** | `POST` | `/api/backup/restore/:id` | Spotify Auth | Ripristina una playlist dallo snapshot |
| **Backup** | `DELETE`| `/api/backup/:id` | Spotify Auth | Elimina manualmente uno snapshot esistente |

---

## 7. Servizi in Background (Cron Jobs & Schedulatori)

I processi in background sono gestiti tramite il plugin nativo **`@elysiajs/cron`** o timer ad alta efficienza nel runtime Bun:

1. **`SnapshotCleanupCron`**:
   - **Intervallo di esecuzione**: Ogni 24 ore (`0 3 * * *` - alle 03:00 UTC).
   - **Compito**: Interroga PostgreSQL tramite Drizzle ORM per rimuovere tutti i record della tabella `playlist_snapshots` creati oltre 30 giorni prima. L'eliminazione a cascata (`ON DELETE CASCADE`) rimuove contestualmente tutti i relativi `snapshot_tracks`.
2. **`RedisCacheMaintenanceCron`**:
   - **Intervallo di esecuzione**: Ogni 5 minuti (`*/5 * * * *`).
   - **Compito**: Esegue un ping di heartbeat sul server Redis, verifica il TTL delle chiavi di cache (`tracks:*`, `playlists:*`, `artists:*`, `spotify_token:*`) e monitora la presenza di chiavi orfane o connesse a sessioni scadute.
3. **`PrefetchWarmingCron`**:
   - **Intervallo di esecuzione**: Ogni 5 minuti (`*/5 * * * *`).
   - **Compito**: Valuta lo stato della cache per utenti attivi ed esegue il prefetching proattivo delle playlist se prossime alla scadenza del TTL.

---

## 8. Requisiti di Configurazione ed Environment

Il sistema richiede la valorizzazione delle seguenti variabili d'ambiente (file `.env` per il backend Bun/Elysia e per il frontend React/Vite):

### 8.1 Backend (`.env`)

| Variabile | Scopo | Esempio / Default |
|---|---|---|
| `PORT` | Porta di ascolto del server ElysiaJS | `3000` |
| `BUN_ENV` | Ambiente di esecuzione (`development` o `production`) | `development` |
| `JWT_SECRET` | Chiave di firma per token JWT (min. 32 caratteri) | `min-32-character-secret-key-for-jwt-signing` |
| `DATABASE_URL` | Stringa di connessione PostgreSQL | `postgres://postgres:password@localhost:5432/tracksbypopularity` |
| `SPOTIFY_CLIENT_ID` | Client ID registrato sulla Spotify Developer Dashboard | `your_spotify_client_id` |
| `SPOTIFY_CLIENT_SECRET` | Client Secret dell'applicazione Spotify | `your_spotify_client_secret` |
| `SPOTIFY_REDIRECT_URI` | Base URI di callback per l'autenticazione Spotify | `http://localhost:3000/auth/callback` |
| `REDIS_HOST` | Hostname del server Redis | `localhost` |
| `REDIS_PORT` | Porta del server Redis | `6379` |
| `REDIS_PASSWORD` | Password di accesso Redis (opzionale) | `redis-secure-pwd` |
| `FRONTEND_ORIGIN` | URL autorizzato nelle policy CORS per il frontend React | `http://localhost:5173` |
| `MAILTRAP_API_KEY` | Chiave API / Credenziali per invio email transazionali | `mailtrap-secret-token` |

### 8.2 Frontend (`.env`)

| Variabile | Scopo | Esempio / Default |
|---|---|---|
| `VITE_API_URL` | URL base del server ElysiaJS | `http://localhost:3000` |
| `VITE_APP_NAME` | Nome dell'applicazione visualizzato nella UI | `TracksByPopularity` |
