if(CMAKE_SYSTEM_PROCESSOR STREQUAL s390x)
  set(CLR_CMAKE_PLATFORM_UNIX_S390X 1)
endif()

if(CLR_CMAKE_PLATFORM_UNIX_S390X)
    set(CLR_CMAKE_PLATFORM_ARCH_S390X 1)
    set(CLR_CMAKE_HOST_ARCH "s390x")
    # The -fms-extensions enable the stuff like __if_exists, __declspec(uuid()), etc.
endif()

if(NOT DEFINED CLR_CMAKE_TARGET_ARCH OR CLR_CMAKE_TARGET_ARCH STREQUAL "" )
  set(CLR_CMAKE_TARGET_ARCH ${CLR_CMAKE_HOST_ARCH})
endif()

if(CLR_CMAKE_TARGET_ARCH STREQUAL s390x)
    set(CLR_CMAKE_TARGET_ARCH_S390X 1)
endif()

if(CLR_CMAKE_TARGET_ARCH_S390X)
  add_definitions(-D_TARGET_S390X_=1)
  add_definitions(-D_TARGET_64BIT_=1)
  add_definitions(-DDBG_TARGET_64BIT=1)
  add_definitions(-DDBG_TARGET_S390X=1)
  add_definitions(-DFEATURE_MULTIREG_RETURN)
  add_compile_definitions($<$<NOT:$<BOOL:$<TARGET_PROPERTY:IGNORE_DEFAULT_TARGET_ARCH>>>:TARGET_S390X>)
endif()
if (CLR_CMAKE_PLATFORM_ARCH_S390X)
  add_definitions(-D_S390X_)
  add_definitions(-DS390X)
  add_definitions(-DBIT64=1)          # CoreClr <= 3.x
  add_definitions(-DHOST_64BIT=1)     # CoreClr > 3.x
  add_definitions(-DHOST_S390X)
  add_definitions(-DBIGENDIAN)
  add_definitions(-DHOST_UNIX)
endif()
