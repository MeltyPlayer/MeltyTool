cmake_minimum_required(VERSION 3.22)
project(AssimpMorphFbxConverter LANGUAGES C CXX)

if(NOT DEFINED ASSIMP_SOURCE_DIR)
  message(FATAL_ERROR "ASSIMP_SOURCE_DIR must point to the patched Assimp source tree")
endif()

set(ASSIMP_BUILD_TESTS OFF CACHE BOOL "" FORCE)
set(ASSIMP_BUILD_ASSIMP_TOOLS OFF CACHE BOOL "" FORCE)
set(ASSIMP_BUILD_SAMPLES OFF CACHE BOOL "" FORCE)
set(ASSIMP_WARNINGS_AS_ERRORS OFF CACHE BOOL "" FORCE)
add_subdirectory("${ASSIMP_SOURCE_DIR}" assimp EXCLUDE_FROM_ALL)

add_executable(assimp_morph_fbx_converter main.cpp)
target_compile_features(assimp_morph_fbx_converter PRIVATE cxx_std_17)
target_link_libraries(assimp_morph_fbx_converter PRIVATE assimp::assimp)
