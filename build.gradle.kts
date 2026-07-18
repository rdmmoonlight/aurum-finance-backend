plugins {
    kotlin("jvm") version "2.4.0"
    id("io.ktor.plugin") version "2.3.12"
}

group = "com.aurumfinance"
version = "0.0.1"

repositories {
    mavenCentral()
}

dependencies {
    implementation("io.ktor:ktor-server-core-jvm:2.3.12")
    implementation("io.ktor:ktor-server-netty-jvm:2.3.12")
    implementation("ch.qos.logback:logback-classic:1.4.14")
    implementation("io.ktor:ktor-server-config-yaml-jvm:2.3.12")
} 