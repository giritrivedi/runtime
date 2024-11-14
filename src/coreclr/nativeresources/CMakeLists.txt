project(nativeresourcestring)

add_library_clr(nativeresourcestring
  STATIC
  resourcestring.cpp
)
include_directories(${CLR_DIR}/minipal)
install_clr (TARGETS nativeresourcestring DESTINATIONS lib)
