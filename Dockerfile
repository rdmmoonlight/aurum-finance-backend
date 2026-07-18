# Tahap 1: Membangun Aplikasi (Build Stage)
FROM gradle:8-jdk17 AS build
COPY --chown=gradle:gradle . /home/gradle/src
WORKDIR /home/gradle/src
# Mengeksekusi pembuatan Fat-Jar (file jar yang sudah berisi semua dependensi)
RUN gradle buildFatJar --no-daemon

# Tahap 2: Menjalankan Aplikasi (Runtime Stage)
FROM amazoncorretto:17-alpine
EXPOSE 8080
RUN mkdir /app
# Menyalin hasil rakitan dari Tahap 1 ke Tahap 2
COPY --from=build /home/gradle/src/build/libs/*-all.jar /app/ktor-app.jar

# Titik eksekusi utama saat server Render menyala
ENTRYPOINT ["java", "-jar", "/app/ktor-app.jar"]