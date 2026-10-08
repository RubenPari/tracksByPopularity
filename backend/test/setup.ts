process.env.NODE_ENV = "test";
process.env.JWT_SECRET ??= "test-secret-that-is-at-least-32-characters";
process.env.TOKEN_ENCRYPTION_KEY ??= "test-token-encryption-key-32chars!!";
process.env.DATABASE_URL ??= "postgres://postgres:password@localhost:5432/tracksbypopularity";
process.env.SPOTIFY_CLIENT_ID ??= "test-client";
process.env.SPOTIFY_CLIENT_SECRET ??= "test-secret";
process.env.SPOTIFY_REDIRECT_URI ??= "http://localhost:3000/auth/callback";
