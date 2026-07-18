pluginManagement {
    repositories {
        mavenCentral()
        gradlePluginPortal()
    }
    kotlin("jvm") version "2.4.0"
}

plugins {
    id("org.gradle.toolchains.foojay-resolver-convention") version "1.0.0"
}

dependencyResolutionManagement {
    repositories {
        mavenCentral()
    }
    versionCatalogs {
        create("ktorLibs").from("io.ktor:ktor-version-catalog:3.5.0")
    }
}

rootProject.name = "backend"

include(":client")
include(":core")
include(":server")
