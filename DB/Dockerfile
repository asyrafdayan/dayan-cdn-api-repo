# Use an official lightweight Alpine Linux image
FROM alpine:latest

# Install SQLite3
RUN apk add --no-cache sqlite

# Set the working directory inside the container
WORKDIR /app

# Copy the existing SQLite database to the working directory inside the container
COPY dev /app/dev

# Set the default command to run sqlite3 with the copied database
# Set the sqlite to run using the copied database
# RUN sqlite3 .open /app/dev
# RUN [".open", "/app/dev"]

CMD ["tail", "-f", "/dev/null"]