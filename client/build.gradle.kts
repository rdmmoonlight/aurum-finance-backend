
plugins {
    alias(libs.plugins.kotlin.jvm) // Mengambil versi otomatis dari toml
    id("io.ktor.plugin") version "2.3.12"
}


kotlin {
    jvm()
    iosArm64()
    iosSimulatorArm64()
    js {
        browser()
    }
    @OptIn(org.jetbrains.kotlin.gradle.ExperimentalWasmDsl::class)
    wasmJs {
        browser()
    }

    sourceSets {
        commonMain.dependencies {
            implementation(ktorLibs.client.core)
            implementation(libs.kotlinx.rpc.client)
            implementation(project(":core"))
        }

    }
}
