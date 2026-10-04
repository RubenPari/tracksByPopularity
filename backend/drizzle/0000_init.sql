CREATE TABLE "playlist_snapshots" (
	"id" uuid PRIMARY KEY DEFAULT gen_random_uuid() NOT NULL,
	"user_id" uuid,
	"spotify_user_id" varchar(255) NOT NULL,
	"playlist_id" varchar(255) NOT NULL,
	"playlist_name" varchar(255) NOT NULL,
	"operation_type" varchar(50) NOT NULL,
	"created_at" timestamp with time zone DEFAULT now() NOT NULL
);
--> statement-breakpoint
CREATE TABLE "snapshot_tracks" (
	"id" serial PRIMARY KEY NOT NULL,
	"snapshot_id" uuid NOT NULL,
	"track_uri" varchar(255) NOT NULL
);
--> statement-breakpoint
CREATE TABLE "spotify_links" (
	"id" uuid PRIMARY KEY DEFAULT gen_random_uuid() NOT NULL,
	"user_id" uuid NOT NULL,
	"spotify_user_id" varchar(255) NOT NULL,
	"access_token" text NOT NULL,
	"refresh_token" text NOT NULL,
	"created_at" timestamp with time zone DEFAULT now() NOT NULL,
	"updated_at" timestamp with time zone DEFAULT now() NOT NULL,
	CONSTRAINT "spotify_links_user_id_unique" UNIQUE("user_id"),
	CONSTRAINT "spotify_links_spotify_user_id_unique" UNIQUE("spotify_user_id")
);
--> statement-breakpoint
CREATE TABLE "users" (
	"id" uuid PRIMARY KEY DEFAULT gen_random_uuid() NOT NULL,
	"email" varchar(255) NOT NULL,
	"password_hash" varchar(255) NOT NULL,
	"created_at" timestamp with time zone DEFAULT now() NOT NULL,
	"updated_at" timestamp with time zone DEFAULT now() NOT NULL
);
--> statement-breakpoint
ALTER TABLE "playlist_snapshots" ADD CONSTRAINT "playlist_snapshots_user_id_users_id_fk" FOREIGN KEY ("user_id") REFERENCES "public"."users"("id") ON DELETE set null ON UPDATE no action;--> statement-breakpoint
ALTER TABLE "snapshot_tracks" ADD CONSTRAINT "snapshot_tracks_snapshot_id_playlist_snapshots_id_fk" FOREIGN KEY ("snapshot_id") REFERENCES "public"."playlist_snapshots"("id") ON DELETE cascade ON UPDATE no action;--> statement-breakpoint
ALTER TABLE "spotify_links" ADD CONSTRAINT "spotify_links_user_id_users_id_fk" FOREIGN KEY ("user_id") REFERENCES "public"."users"("id") ON DELETE cascade ON UPDATE no action;--> statement-breakpoint
CREATE INDEX "playlist_snapshots_spotify_user_idx" ON "playlist_snapshots" USING btree ("spotify_user_id");--> statement-breakpoint
CREATE INDEX "playlist_snapshots_created_at_idx" ON "playlist_snapshots" USING btree ("created_at");--> statement-breakpoint
CREATE INDEX "snapshot_tracks_snapshot_idx" ON "snapshot_tracks" USING btree ("snapshot_id");--> statement-breakpoint
CREATE UNIQUE INDEX "users_email_idx" ON "users" USING btree ("email");