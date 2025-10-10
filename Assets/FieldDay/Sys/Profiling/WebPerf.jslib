var WebPerf = {
    WebPerf_IsCrossOriginIsolated__sig: 'i',
    WebPerf_IsCrossOriginIsolated: function() {
        return window.crossOriginIsolated;
    },

    WebPerf_GetMemoryUsage__sig: 'i',
    WebPerf_GetMemoryUsage: function() {
        return unityInstance.GetMemoryInfo().usedWASMHeapSize;
    }
};

mergeInto(LibraryManager.library, WebPerf);