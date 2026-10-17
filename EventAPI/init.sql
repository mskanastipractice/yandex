CREATE USER "auth_serviceuser" WITH PASSWORD 'auth_serviceuser';
CREATE USER "events_serviceuser" WITH PASSWORD 'events_serviceuser';
CREATE USER "bookings_serviceuser" WITH PASSWORD 'bookings_serviceuser';

CREATE DATABASE "auth" WITH OWNER "auth_serviceuser";
CREATE DATABASE "events" WITH OWNER "events_serviceuser";
CREATE DATABASE "bookings" WITH OWNER "bookings_serviceuser";
