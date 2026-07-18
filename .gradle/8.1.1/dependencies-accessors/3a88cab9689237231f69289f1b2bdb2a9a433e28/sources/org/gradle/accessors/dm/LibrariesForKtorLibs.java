package org.gradle.accessors.dm;

import org.gradle.api.NonNullApi;
import org.gradle.api.artifacts.MinimalExternalModuleDependency;
import org.gradle.plugin.use.PluginDependency;
import org.gradle.api.artifacts.ExternalModuleDependencyBundle;
import org.gradle.api.artifacts.MutableVersionConstraint;
import org.gradle.api.provider.Provider;
import org.gradle.api.model.ObjectFactory;
import org.gradle.api.provider.ProviderFactory;
import org.gradle.api.internal.catalog.AbstractExternalDependencyFactory;
import org.gradle.api.internal.catalog.DefaultVersionCatalog;
import java.util.Map;
import org.gradle.api.internal.attributes.ImmutableAttributesFactory;
import org.gradle.api.internal.artifacts.dsl.CapabilityNotationParser;
import javax.inject.Inject;

/**
 * A catalog of dependencies accessible via the `ktorLibs` extension.
 */
@NonNullApi
public class LibrariesForKtorLibs extends AbstractExternalDependencyFactory {

    private final AbstractExternalDependencyFactory owner = this;
    private final ClientLibraryAccessors laccForClientLibraryAccessors = new ClientLibraryAccessors(owner);
    private final HtmxLibraryAccessors laccForHtmxLibraryAccessors = new HtmxLibraryAccessors(owner);
    private final HttpLibraryAccessors laccForHttpLibraryAccessors = new HttpLibraryAccessors(owner);
    private final NetworkLibraryAccessors laccForNetworkLibraryAccessors = new NetworkLibraryAccessors(owner);
    private final OpenapiSchemaLibraryAccessors laccForOpenapiSchemaLibraryAccessors = new OpenapiSchemaLibraryAccessors(owner);
    private final SerializationLibraryAccessors laccForSerializationLibraryAccessors = new SerializationLibraryAccessors(owner);
    private final ServerLibraryAccessors laccForServerLibraryAccessors = new ServerLibraryAccessors(owner);
    private final WebsocketsLibraryAccessors laccForWebsocketsLibraryAccessors = new WebsocketsLibraryAccessors(owner);
    private final VersionAccessors vaccForVersionAccessors = new VersionAccessors(providers, config);
    private final BundleAccessors baccForBundleAccessors = new BundleAccessors(objects, providers, config, attributesFactory, capabilityNotationParser);
    private final PluginAccessors paccForPluginAccessors = new PluginAccessors(providers, config);

    @Inject
    public LibrariesForKtorLibs(DefaultVersionCatalog config, ProviderFactory providers, ObjectFactory objects, ImmutableAttributesFactory attributesFactory, CapabilityNotationParser capabilityNotationParser) {
        super(config, providers, objects, attributesFactory, capabilityNotationParser);
    }

        /**
         * Creates a dependency provider for bom (io.ktor:ktor-bom)
         * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
         */
        public Provider<MinimalExternalModuleDependency> getBom() {
            return create("bom");
    }

        /**
         * Creates a dependency provider for callId (io.ktor:ktor-call-id)
         * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
         */
        public Provider<MinimalExternalModuleDependency> getCallId() {
            return create("callId");
    }

        /**
         * Creates a dependency provider for compilerPlugin (io.ktor:ktor-compiler-plugin)
         * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
         */
        public Provider<MinimalExternalModuleDependency> getCompilerPlugin() {
            return create("compilerPlugin");
    }

        /**
         * Creates a dependency provider for encodingZstd (io.ktor:ktor-encoding-zstd)
         * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
         */
        public Provider<MinimalExternalModuleDependency> getEncodingZstd() {
            return create("encodingZstd");
    }

        /**
         * Creates a dependency provider for events (io.ktor:ktor-events)
         * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
         */
        public Provider<MinimalExternalModuleDependency> getEvents() {
            return create("events");
    }

        /**
         * Creates a dependency provider for gradlePlugin (io.ktor.plugin:plugin)
         * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
         */
        public Provider<MinimalExternalModuleDependency> getGradlePlugin() {
            return create("gradlePlugin");
    }

        /**
         * Creates a dependency provider for io (io.ktor:ktor-io)
         * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
         */
        public Provider<MinimalExternalModuleDependency> getIo() {
            return create("io");
    }

        /**
         * Creates a dependency provider for resources (io.ktor:ktor-resources)
         * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
         */
        public Provider<MinimalExternalModuleDependency> getResources() {
            return create("resources");
    }

        /**
         * Creates a dependency provider for sse (io.ktor:ktor-sse)
         * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
         */
        public Provider<MinimalExternalModuleDependency> getSse() {
            return create("sse");
    }

        /**
         * Creates a dependency provider for testDispatcher (io.ktor:ktor-test-dispatcher)
         * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
         */
        public Provider<MinimalExternalModuleDependency> getTestDispatcher() {
            return create("testDispatcher");
    }

        /**
         * Creates a dependency provider for utils (io.ktor:ktor-utils)
         * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
         */
        public Provider<MinimalExternalModuleDependency> getUtils() {
            return create("utils");
    }

    /**
     * Returns the group of libraries at client
     */
    public ClientLibraryAccessors getClient() {
        return laccForClientLibraryAccessors;
    }

    /**
     * Returns the group of libraries at htmx
     */
    public HtmxLibraryAccessors getHtmx() {
        return laccForHtmxLibraryAccessors;
    }

    /**
     * Returns the group of libraries at http
     */
    public HttpLibraryAccessors getHttp() {
        return laccForHttpLibraryAccessors;
    }

    /**
     * Returns the group of libraries at network
     */
    public NetworkLibraryAccessors getNetwork() {
        return laccForNetworkLibraryAccessors;
    }

    /**
     * Returns the group of libraries at openapiSchema
     */
    public OpenapiSchemaLibraryAccessors getOpenapiSchema() {
        return laccForOpenapiSchemaLibraryAccessors;
    }

    /**
     * Returns the group of libraries at serialization
     */
    public SerializationLibraryAccessors getSerialization() {
        return laccForSerializationLibraryAccessors;
    }

    /**
     * Returns the group of libraries at server
     */
    public ServerLibraryAccessors getServer() {
        return laccForServerLibraryAccessors;
    }

    /**
     * Returns the group of libraries at websockets
     */
    public WebsocketsLibraryAccessors getWebsockets() {
        return laccForWebsocketsLibraryAccessors;
    }

    /**
     * Returns the group of versions at versions
     */
    public VersionAccessors getVersions() {
        return vaccForVersionAccessors;
    }

    /**
     * Returns the group of bundles at bundles
     */
    public BundleAccessors getBundles() {
        return baccForBundleAccessors;
    }

    /**
     * Returns the group of plugins at plugins
     */
    public PluginAccessors getPlugins() {
        return paccForPluginAccessors;
    }

    public static class ClientLibraryAccessors extends SubDependencyFactory {
        private final ClientDarwinLibraryAccessors laccForClientDarwinLibraryAccessors = new ClientDarwinLibraryAccessors(owner);
        private final ClientJettyLibraryAccessors laccForClientJettyLibraryAccessors = new ClientJettyLibraryAccessors(owner);

        public ClientLibraryAccessors(AbstractExternalDependencyFactory owner) { super(owner); }

            /**
             * Creates a dependency provider for android (io.ktor:ktor-client-android)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getAndroid() {
                return create("client.android");
        }

            /**
             * Creates a dependency provider for apache (io.ktor:ktor-client-apache)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getApache() {
                return create("client.apache");
        }

            /**
             * Creates a dependency provider for apache5 (io.ktor:ktor-client-apache5)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getApache5() {
                return create("client.apache5");
        }

            /**
             * Creates a dependency provider for auth (io.ktor:ktor-client-auth)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getAuth() {
                return create("client.auth");
        }

            /**
             * Creates a dependency provider for bomRemover (io.ktor:ktor-client-bom-remover)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getBomRemover() {
                return create("client.bomRemover");
        }

            /**
             * Creates a dependency provider for callId (io.ktor:ktor-client-call-id)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getCallId() {
                return create("client.callId");
        }

            /**
             * Creates a dependency provider for cio (io.ktor:ktor-client-cio)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getCio() {
                return create("client.cio");
        }

            /**
             * Creates a dependency provider for contentNegotiation (io.ktor:ktor-client-content-negotiation)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getContentNegotiation() {
                return create("client.contentNegotiation");
        }

            /**
             * Creates a dependency provider for core (io.ktor:ktor-client-core)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getCore() {
                return create("client.core");
        }

            /**
             * Creates a dependency provider for curl (io.ktor:ktor-client-curl)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getCurl() {
                return create("client.curl");
        }

            /**
             * Creates a dependency provider for encoding (io.ktor:ktor-client-encoding)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getEncoding() {
                return create("client.encoding");
        }

            /**
             * Creates a dependency provider for gson (io.ktor:ktor-client-gson)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getGson() {
                return create("client.gson");
        }

            /**
             * Creates a dependency provider for ios (io.ktor:ktor-client-ios)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getIos() {
                return create("client.ios");
        }

            /**
             * Creates a dependency provider for jackson (io.ktor:ktor-client-jackson)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getJackson() {
                return create("client.jackson");
        }

            /**
             * Creates a dependency provider for java (io.ktor:ktor-client-java)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getJava() {
                return create("client.java");
        }

            /**
             * Creates a dependency provider for js (io.ktor:ktor-client-js)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getJs() {
                return create("client.js");
        }

            /**
             * Creates a dependency provider for json (io.ktor:ktor-client-json)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getJson() {
                return create("client.json");
        }

            /**
             * Creates a dependency provider for logging (io.ktor:ktor-client-logging)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getLogging() {
                return create("client.logging");
        }

            /**
             * Creates a dependency provider for mock (io.ktor:ktor-client-mock)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getMock() {
                return create("client.mock");
        }

            /**
             * Creates a dependency provider for okhttp (io.ktor:ktor-client-okhttp)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getOkhttp() {
                return create("client.okhttp");
        }

            /**
             * Creates a dependency provider for resources (io.ktor:ktor-client-resources)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getResources() {
                return create("client.resources");
        }

            /**
             * Creates a dependency provider for serialization (io.ktor:ktor-client-serialization)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getSerialization() {
                return create("client.serialization");
        }

            /**
             * Creates a dependency provider for webrtc (io.ktor:ktor-client-webrtc)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getWebrtc() {
                return create("client.webrtc");
        }

            /**
             * Creates a dependency provider for websockets (io.ktor:ktor-client-websockets)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getWebsockets() {
                return create("client.websockets");
        }

            /**
             * Creates a dependency provider for winhttp (io.ktor:ktor-client-winhttp)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getWinhttp() {
                return create("client.winhttp");
        }

        /**
         * Returns the group of libraries at client.darwin
         */
        public ClientDarwinLibraryAccessors getDarwin() {
            return laccForClientDarwinLibraryAccessors;
        }

        /**
         * Returns the group of libraries at client.jetty
         */
        public ClientJettyLibraryAccessors getJetty() {
            return laccForClientJettyLibraryAccessors;
        }

    }

    public static class ClientDarwinLibraryAccessors extends SubDependencyFactory implements DependencyNotationSupplier {

        public ClientDarwinLibraryAccessors(AbstractExternalDependencyFactory owner) { super(owner); }

            /**
             * Creates a dependency provider for darwin (io.ktor:ktor-client-darwin)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> asProvider() {
                return create("client.darwin");
        }

            /**
             * Creates a dependency provider for legacy (io.ktor:ktor-client-darwin-legacy)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getLegacy() {
                return create("client.darwin.legacy");
        }

    }

    public static class ClientJettyLibraryAccessors extends SubDependencyFactory implements DependencyNotationSupplier {

        public ClientJettyLibraryAccessors(AbstractExternalDependencyFactory owner) { super(owner); }

            /**
             * Creates a dependency provider for jetty (io.ktor:ktor-client-jetty-jakarta)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> asProvider() {
                return create("client.jetty");
        }

            /**
             * Creates a dependency provider for legacy (io.ktor:ktor-client-jetty)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getLegacy() {
                return create("client.jetty.legacy");
        }

    }

    public static class HtmxLibraryAccessors extends SubDependencyFactory implements DependencyNotationSupplier {

        public HtmxLibraryAccessors(AbstractExternalDependencyFactory owner) { super(owner); }

            /**
             * Creates a dependency provider for htmx (io.ktor:ktor-htmx)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> asProvider() {
                return create("htmx");
        }

            /**
             * Creates a dependency provider for html (io.ktor:ktor-htmx-html)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getHtml() {
                return create("htmx.html");
        }

    }

    public static class HttpLibraryAccessors extends SubDependencyFactory implements DependencyNotationSupplier {

        public HttpLibraryAccessors(AbstractExternalDependencyFactory owner) { super(owner); }

            /**
             * Creates a dependency provider for http (io.ktor:ktor-http)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> asProvider() {
                return create("http");
        }

            /**
             * Creates a dependency provider for cio (io.ktor:ktor-http-cio)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getCio() {
                return create("http.cio");
        }

    }

    public static class NetworkLibraryAccessors extends SubDependencyFactory implements DependencyNotationSupplier {
        private final NetworkTlsLibraryAccessors laccForNetworkTlsLibraryAccessors = new NetworkTlsLibraryAccessors(owner);

        public NetworkLibraryAccessors(AbstractExternalDependencyFactory owner) { super(owner); }

            /**
             * Creates a dependency provider for network (io.ktor:ktor-network)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> asProvider() {
                return create("network");
        }

        /**
         * Returns the group of libraries at network.tls
         */
        public NetworkTlsLibraryAccessors getTls() {
            return laccForNetworkTlsLibraryAccessors;
        }

    }

    public static class NetworkTlsLibraryAccessors extends SubDependencyFactory implements DependencyNotationSupplier {

        public NetworkTlsLibraryAccessors(AbstractExternalDependencyFactory owner) { super(owner); }

            /**
             * Creates a dependency provider for tls (io.ktor:ktor-network-tls)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> asProvider() {
                return create("network.tls");
        }

            /**
             * Creates a dependency provider for certificates (io.ktor:ktor-network-tls-certificates)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getCertificates() {
                return create("network.tls.certificates");
        }

    }

    public static class OpenapiSchemaLibraryAccessors extends SubDependencyFactory implements DependencyNotationSupplier {

        public OpenapiSchemaLibraryAccessors(AbstractExternalDependencyFactory owner) { super(owner); }

            /**
             * Creates a dependency provider for openapiSchema (io.ktor:ktor-openapi-schema)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> asProvider() {
                return create("openapiSchema");
        }

            /**
             * Creates a dependency provider for reflect (io.ktor:ktor-openapi-schema-reflect)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getReflect() {
                return create("openapiSchema.reflect");
        }

    }

    public static class SerializationLibraryAccessors extends SubDependencyFactory implements DependencyNotationSupplier {
        private final SerializationKotlinxLibraryAccessors laccForSerializationKotlinxLibraryAccessors = new SerializationKotlinxLibraryAccessors(owner);

        public SerializationLibraryAccessors(AbstractExternalDependencyFactory owner) { super(owner); }

            /**
             * Creates a dependency provider for serialization (io.ktor:ktor-serialization)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> asProvider() {
                return create("serialization");
        }

            /**
             * Creates a dependency provider for gson (io.ktor:ktor-serialization-gson)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getGson() {
                return create("serialization.gson");
        }

            /**
             * Creates a dependency provider for jackson (io.ktor:ktor-serialization-jackson)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getJackson() {
                return create("serialization.jackson");
        }

            /**
             * Creates a dependency provider for jackson3 (io.ktor:ktor-serialization-jackson3)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getJackson3() {
                return create("serialization.jackson3");
        }

        /**
         * Returns the group of libraries at serialization.kotlinx
         */
        public SerializationKotlinxLibraryAccessors getKotlinx() {
            return laccForSerializationKotlinxLibraryAccessors;
        }

    }

    public static class SerializationKotlinxLibraryAccessors extends SubDependencyFactory implements DependencyNotationSupplier {

        public SerializationKotlinxLibraryAccessors(AbstractExternalDependencyFactory owner) { super(owner); }

            /**
             * Creates a dependency provider for kotlinx (io.ktor:ktor-serialization-kotlinx)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> asProvider() {
                return create("serialization.kotlinx");
        }

            /**
             * Creates a dependency provider for cbor (io.ktor:ktor-serialization-kotlinx-cbor)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getCbor() {
                return create("serialization.kotlinx.cbor");
        }

            /**
             * Creates a dependency provider for json (io.ktor:ktor-serialization-kotlinx-json)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getJson() {
                return create("serialization.kotlinx.json");
        }

            /**
             * Creates a dependency provider for protobuf (io.ktor:ktor-serialization-kotlinx-protobuf)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getProtobuf() {
                return create("serialization.kotlinx.protobuf");
        }

            /**
             * Creates a dependency provider for xml (io.ktor:ktor-serialization-kotlinx-xml)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getXml() {
                return create("serialization.kotlinx.xml");
        }

    }

    public static class ServerLibraryAccessors extends SubDependencyFactory {
        private final ServerAuthLibraryAccessors laccForServerAuthLibraryAccessors = new ServerAuthLibraryAccessors(owner);
        private final ServerCompressionLibraryAccessors laccForServerCompressionLibraryAccessors = new ServerCompressionLibraryAccessors(owner);
        private final ServerConfigLibraryAccessors laccForServerConfigLibraryAccessors = new ServerConfigLibraryAccessors(owner);
        private final ServerJettyLibraryAccessors laccForServerJettyLibraryAccessors = new ServerJettyLibraryAccessors(owner);
        private final ServerMetricsLibraryAccessors laccForServerMetricsLibraryAccessors = new ServerMetricsLibraryAccessors(owner);
        private final ServerServletLibraryAccessors laccForServerServletLibraryAccessors = new ServerServletLibraryAccessors(owner);
        private final ServerTomcatLibraryAccessors laccForServerTomcatLibraryAccessors = new ServerTomcatLibraryAccessors(owner);

        public ServerLibraryAccessors(AbstractExternalDependencyFactory owner) { super(owner); }

            /**
             * Creates a dependency provider for autoHeadResponse (io.ktor:ktor-server-auto-head-response)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getAutoHeadResponse() {
                return create("server.autoHeadResponse");
        }

            /**
             * Creates a dependency provider for bodyLimit (io.ktor:ktor-server-body-limit)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getBodyLimit() {
                return create("server.bodyLimit");
        }

            /**
             * Creates a dependency provider for cachingHeaders (io.ktor:ktor-server-caching-headers)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getCachingHeaders() {
                return create("server.cachingHeaders");
        }

            /**
             * Creates a dependency provider for callId (io.ktor:ktor-server-call-id)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getCallId() {
                return create("server.callId");
        }

            /**
             * Creates a dependency provider for callLogging (io.ktor:ktor-server-call-logging)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getCallLogging() {
                return create("server.callLogging");
        }

            /**
             * Creates a dependency provider for cio (io.ktor:ktor-server-cio)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getCio() {
                return create("server.cio");
        }

            /**
             * Creates a dependency provider for conditionalHeaders (io.ktor:ktor-server-conditional-headers)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getConditionalHeaders() {
                return create("server.conditionalHeaders");
        }

            /**
             * Creates a dependency provider for contentNegotiation (io.ktor:ktor-server-content-negotiation)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getContentNegotiation() {
                return create("server.contentNegotiation");
        }

            /**
             * Creates a dependency provider for core (io.ktor:ktor-server-core)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getCore() {
                return create("server.core");
        }

            /**
             * Creates a dependency provider for cors (io.ktor:ktor-server-cors)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getCors() {
                return create("server.cors");
        }

            /**
             * Creates a dependency provider for csrf (io.ktor:ktor-server-csrf)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getCsrf() {
                return create("server.csrf");
        }

            /**
             * Creates a dependency provider for dataConversion (io.ktor:ktor-server-data-conversion)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getDataConversion() {
                return create("server.dataConversion");
        }

            /**
             * Creates a dependency provider for defaultHeaders (io.ktor:ktor-server-default-headers)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getDefaultHeaders() {
                return create("server.defaultHeaders");
        }

            /**
             * Creates a dependency provider for di (io.ktor:ktor-server-di)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getDi() {
                return create("server.di");
        }

            /**
             * Creates a dependency provider for doubleReceive (io.ktor:ktor-server-double-receive)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getDoubleReceive() {
                return create("server.doubleReceive");
        }

            /**
             * Creates a dependency provider for forwardedHeader (io.ktor:ktor-server-forwarded-header)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getForwardedHeader() {
                return create("server.forwardedHeader");
        }

            /**
             * Creates a dependency provider for freemarker (io.ktor:ktor-server-freemarker)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getFreemarker() {
                return create("server.freemarker");
        }

            /**
             * Creates a dependency provider for hsts (io.ktor:ktor-server-hsts)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getHsts() {
                return create("server.hsts");
        }

            /**
             * Creates a dependency provider for htmlBuilder (io.ktor:ktor-server-html-builder)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getHtmlBuilder() {
                return create("server.htmlBuilder");
        }

            /**
             * Creates a dependency provider for htmx (io.ktor:ktor-server-htmx)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getHtmx() {
                return create("server.htmx");
        }

            /**
             * Creates a dependency provider for httpRedirect (io.ktor:ktor-server-http-redirect)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getHttpRedirect() {
                return create("server.httpRedirect");
        }

            /**
             * Creates a dependency provider for i18n (io.ktor:ktor-server-i18n)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getI18n() {
                return create("server.i18n");
        }

            /**
             * Creates a dependency provider for jte (io.ktor:ktor-server-jte)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getJte() {
                return create("server.jte");
        }

            /**
             * Creates a dependency provider for methodOverride (io.ktor:ktor-server-method-override)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getMethodOverride() {
                return create("server.methodOverride");
        }

            /**
             * Creates a dependency provider for mustache (io.ktor:ktor-server-mustache)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getMustache() {
                return create("server.mustache");
        }

            /**
             * Creates a dependency provider for netty (io.ktor:ktor-server-netty)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getNetty() {
                return create("server.netty");
        }

            /**
             * Creates a dependency provider for openapi (io.ktor:ktor-server-openapi)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getOpenapi() {
                return create("server.openapi");
        }

            /**
             * Creates a dependency provider for partialContent (io.ktor:ktor-server-partial-content)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getPartialContent() {
                return create("server.partialContent");
        }

            /**
             * Creates a dependency provider for pebble (io.ktor:ktor-server-pebble)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getPebble() {
                return create("server.pebble");
        }

            /**
             * Creates a dependency provider for rateLimit (io.ktor:ktor-server-rate-limit)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getRateLimit() {
                return create("server.rateLimit");
        }

            /**
             * Creates a dependency provider for requestValidation (io.ktor:ktor-server-request-validation)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getRequestValidation() {
                return create("server.requestValidation");
        }

            /**
             * Creates a dependency provider for resources (io.ktor:ktor-server-resources)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getResources() {
                return create("server.resources");
        }

            /**
             * Creates a dependency provider for routingOpenapi (io.ktor:ktor-server-routing-openapi)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getRoutingOpenapi() {
                return create("server.routingOpenapi");
        }

            /**
             * Creates a dependency provider for sessions (io.ktor:ktor-server-sessions)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getSessions() {
                return create("server.sessions");
        }

            /**
             * Creates a dependency provider for sse (io.ktor:ktor-server-sse)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getSse() {
                return create("server.sse");
        }

            /**
             * Creates a dependency provider for statusPages (io.ktor:ktor-server-status-pages)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getStatusPages() {
                return create("server.statusPages");
        }

            /**
             * Creates a dependency provider for swagger (io.ktor:ktor-server-swagger)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getSwagger() {
                return create("server.swagger");
        }

            /**
             * Creates a dependency provider for testHost (io.ktor:ktor-server-test-host)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getTestHost() {
                return create("server.testHost");
        }

            /**
             * Creates a dependency provider for thymeleaf (io.ktor:ktor-server-thymeleaf)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getThymeleaf() {
                return create("server.thymeleaf");
        }

            /**
             * Creates a dependency provider for velocity (io.ktor:ktor-server-velocity)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getVelocity() {
                return create("server.velocity");
        }

            /**
             * Creates a dependency provider for webjars (io.ktor:ktor-server-webjars)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getWebjars() {
                return create("server.webjars");
        }

            /**
             * Creates a dependency provider for websockets (io.ktor:ktor-server-websockets)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getWebsockets() {
                return create("server.websockets");
        }

        /**
         * Returns the group of libraries at server.auth
         */
        public ServerAuthLibraryAccessors getAuth() {
            return laccForServerAuthLibraryAccessors;
        }

        /**
         * Returns the group of libraries at server.compression
         */
        public ServerCompressionLibraryAccessors getCompression() {
            return laccForServerCompressionLibraryAccessors;
        }

        /**
         * Returns the group of libraries at server.config
         */
        public ServerConfigLibraryAccessors getConfig() {
            return laccForServerConfigLibraryAccessors;
        }

        /**
         * Returns the group of libraries at server.jetty
         */
        public ServerJettyLibraryAccessors getJetty() {
            return laccForServerJettyLibraryAccessors;
        }

        /**
         * Returns the group of libraries at server.metrics
         */
        public ServerMetricsLibraryAccessors getMetrics() {
            return laccForServerMetricsLibraryAccessors;
        }

        /**
         * Returns the group of libraries at server.servlet
         */
        public ServerServletLibraryAccessors getServlet() {
            return laccForServerServletLibraryAccessors;
        }

        /**
         * Returns the group of libraries at server.tomcat
         */
        public ServerTomcatLibraryAccessors getTomcat() {
            return laccForServerTomcatLibraryAccessors;
        }

    }

    public static class ServerAuthLibraryAccessors extends SubDependencyFactory implements DependencyNotationSupplier {

        public ServerAuthLibraryAccessors(AbstractExternalDependencyFactory owner) { super(owner); }

            /**
             * Creates a dependency provider for auth (io.ktor:ktor-server-auth)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> asProvider() {
                return create("server.auth");
        }

            /**
             * Creates a dependency provider for apiKey (io.ktor:ktor-server-auth-api-key)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getApiKey() {
                return create("server.auth.apiKey");
        }

            /**
             * Creates a dependency provider for jwt (io.ktor:ktor-server-auth-jwt)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getJwt() {
                return create("server.auth.jwt");
        }

            /**
             * Creates a dependency provider for ldap (io.ktor:ktor-server-auth-ldap)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getLdap() {
                return create("server.auth.ldap");
        }

    }

    public static class ServerCompressionLibraryAccessors extends SubDependencyFactory implements DependencyNotationSupplier {

        public ServerCompressionLibraryAccessors(AbstractExternalDependencyFactory owner) { super(owner); }

            /**
             * Creates a dependency provider for compression (io.ktor:ktor-server-compression)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> asProvider() {
                return create("server.compression");
        }

            /**
             * Creates a dependency provider for zstd (io.ktor:ktor-server-compression-zstd)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getZstd() {
                return create("server.compression.zstd");
        }

    }

    public static class ServerConfigLibraryAccessors extends SubDependencyFactory {

        public ServerConfigLibraryAccessors(AbstractExternalDependencyFactory owner) { super(owner); }

            /**
             * Creates a dependency provider for yaml (io.ktor:ktor-server-config-yaml)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getYaml() {
                return create("server.config.yaml");
        }

    }

    public static class ServerJettyLibraryAccessors extends SubDependencyFactory implements DependencyNotationSupplier {

        public ServerJettyLibraryAccessors(AbstractExternalDependencyFactory owner) { super(owner); }

            /**
             * Creates a dependency provider for jetty (io.ktor:ktor-server-jetty-jakarta)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> asProvider() {
                return create("server.jetty");
        }

            /**
             * Creates a dependency provider for legacy (io.ktor:ktor-server-jetty)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getLegacy() {
                return create("server.jetty.legacy");
        }

    }

    public static class ServerMetricsLibraryAccessors extends SubDependencyFactory implements DependencyNotationSupplier {

        public ServerMetricsLibraryAccessors(AbstractExternalDependencyFactory owner) { super(owner); }

            /**
             * Creates a dependency provider for metrics (io.ktor:ktor-server-metrics)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> asProvider() {
                return create("server.metrics");
        }

            /**
             * Creates a dependency provider for micrometer (io.ktor:ktor-server-metrics-micrometer)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getMicrometer() {
                return create("server.metrics.micrometer");
        }

    }

    public static class ServerServletLibraryAccessors extends SubDependencyFactory implements DependencyNotationSupplier {

        public ServerServletLibraryAccessors(AbstractExternalDependencyFactory owner) { super(owner); }

            /**
             * Creates a dependency provider for servlet (io.ktor:ktor-server-servlet-jakarta)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> asProvider() {
                return create("server.servlet");
        }

            /**
             * Creates a dependency provider for legacy (io.ktor:ktor-server-servlet)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getLegacy() {
                return create("server.servlet.legacy");
        }

    }

    public static class ServerTomcatLibraryAccessors extends SubDependencyFactory implements DependencyNotationSupplier {

        public ServerTomcatLibraryAccessors(AbstractExternalDependencyFactory owner) { super(owner); }

            /**
             * Creates a dependency provider for tomcat (io.ktor:ktor-server-tomcat-jakarta)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> asProvider() {
                return create("server.tomcat");
        }

            /**
             * Creates a dependency provider for legacy (io.ktor:ktor-server-tomcat)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getLegacy() {
                return create("server.tomcat.legacy");
        }

    }

    public static class WebsocketsLibraryAccessors extends SubDependencyFactory implements DependencyNotationSupplier {

        public WebsocketsLibraryAccessors(AbstractExternalDependencyFactory owner) { super(owner); }

            /**
             * Creates a dependency provider for websockets (io.ktor:ktor-websockets)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> asProvider() {
                return create("websockets");
        }

            /**
             * Creates a dependency provider for serialization (io.ktor:ktor-websocket-serialization)
             * This dependency was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<MinimalExternalModuleDependency> getSerialization() {
                return create("websockets.serialization");
        }

    }

    public static class VersionAccessors extends VersionFactory  {

        public VersionAccessors(ProviderFactory providers, DefaultVersionCatalog config) { super(providers, config); }

            /**
             * Returns the version associated to this alias: ktor (3.5.0)
             * If the version is a rich version and that its not expressible as a
             * single version string, then an empty string is returned.
             * This version was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<String> getKtor() { return getVersion("ktor"); }

    }

    public static class BundleAccessors extends BundleFactory {

        public BundleAccessors(ObjectFactory objects, ProviderFactory providers, DefaultVersionCatalog config, ImmutableAttributesFactory attributesFactory, CapabilityNotationParser capabilityNotationParser) { super(objects, providers, config, attributesFactory, capabilityNotationParser); }

    }

    public static class PluginAccessors extends PluginFactory {

        public PluginAccessors(ProviderFactory providers, DefaultVersionCatalog config) { super(providers, config); }

            /**
             * Creates a plugin provider for ktor to the plugin id 'io.ktor.plugin'
             * This plugin was declared in catalog io.ktor:ktor-version-catalog:3.5.0
             */
            public Provider<PluginDependency> getKtor() { return createPlugin("ktor"); }

    }

}
