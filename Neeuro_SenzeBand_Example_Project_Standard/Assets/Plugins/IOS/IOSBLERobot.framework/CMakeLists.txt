# For more information about using CMake with Android Studio, read the
# documentation: https://d.android.com/studio/projects/add-native-code.html

# Sets the minimum version of CMake required to build the native library.

cmake_minimum_required(VERSION 3.10.2)

set (CMAKE_CXX_STANDARD 11)
set (CMAKE_CXX_STANDARD_REQUIRED on)

file(GLOB SP_FILES "*.c")
file(GLOB_RECURSE C_FILES "StressRelax/*.c")

#add_executable(testAnalyze Testing/testAnalyze.cxx)

#if(TARGET testAnalyze)
#    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -DNEEURO_SDK=1")
#endif()

# Creates and names a library, sets it as either STATIC
# or SHARED, and provides the relative paths to its source code.
# You can define multiple libraries, and CMake builds them for you.
# Gradle automatically packages shared libraries with your APK.


add_library(
        # Sets the name of the library.
        Sigpro-lib

        # Sets the library as a shared library.
        SHARED

        # Provides a relative path to your source file(s).
        iir.c
        utils.c
        fft_complex.c
        ComplexNumber.cpp
        AnalyseEEG.cpp
        JNIBridge/native-lib.cpp
        JNIBridge/unity-lib.cpp
        JNIBridge/JNIBridge.cpp
        Meanline.cpp
        Senzeband.cpp
        SDKVersionControl.cpp
        utils_cpp.cpp
        SVMConstants.cpp
        SVMConstantsV2.cpp
        SVMConstantsV100.cpp
        FilterV2.cpp)

add_library(
        cAlgorithm
        SHARED
        ${C_FILES}
)

#add_library(
#        JNIbridge
#        SHARED
#        native-lib.cpp
#)

# Searches for a specified prebuilt library and stores the path as a
# variable. Because CMake includes system libraries in the search path by
# default, you only need to specify the name of the public NDK library
# you want to add. CMake verifies that the library exists before
# completing its build.

find_library( # Sets the name of the path variable.
        log-lib

        # Specifies the name of the NDK library that
        # you want CMake to locate.
        log)

# Specifies libraries CMake should link to your target library. You
# can link multiple libraries, such as libraries you define in this
# build script, prebuilt third-party libraries, or system libraries.


target_link_libraries( # Specifies the target library.
        Sigpro-lib
        cAlgorithm

        #        JNIbridge

        #android

        # Links the target library to the log library
        # included in the NDK.
        ${log-lib}
        )

#if(TARGET testAnalyze)
#    target_link_libraries(testAnalyze Sigpro-lib)
#endif()
