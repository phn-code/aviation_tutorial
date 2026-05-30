if(NOT TARGET OpenXR::headers)
add_library(OpenXR::headers INTERFACE IMPORTED)
set_target_properties(OpenXR::headers PROPERTIES
    INTERFACE_INCLUDE_DIRECTORIES "/Users/phuchaduynguyen/.gradle/caches/8.11/transforms/d00d8356610de82ad2f353844ca63358/transformed/jetified-openxr_loader/prefab/modules/headers/include"
    INTERFACE_LINK_LIBRARIES ""
)
endif()

if(NOT TARGET OpenXR::openxr_loader)
add_library(OpenXR::openxr_loader SHARED IMPORTED)
set_target_properties(OpenXR::openxr_loader PROPERTIES
    IMPORTED_LOCATION "/Users/phuchaduynguyen/.gradle/caches/8.11/transforms/d00d8356610de82ad2f353844ca63358/transformed/jetified-openxr_loader/prefab/modules/openxr_loader/libs/android.arm64-v8a/libopenxr_loader.so"
    INTERFACE_LINK_LIBRARIES "OpenXR::headers"
)
endif()

