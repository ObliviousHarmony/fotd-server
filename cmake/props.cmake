# cmake/props.cmake
# Generates a Visual Studio .props file with ClCompile / ClInclude entries
# and optional include directories. Preserves folder structure as filters.

function(generate_vcx_props target_name)
  cmake_parse_arguments(
    GEN        # options (none)
    ""         # single-value arguments
    ""         # multi-value args (none)
    "SOURCES;HEADERS;TESTS;INCLUDES" # multi-value arguments
    ${ARGN}
  )

  # We must create filters for all of the directories so that the files can be
  # sorted correctly in Visual Studio. We also include the files themselves
  # so that they can be sorted into directories correctly.
  message(STATUS "Generating .vcxproj.filters for target: ${target_name}")

  set(filters_file "${CMAKE_CURRENT_SOURCE_DIR}/${target_name}.vcxproj.filters")
  file(WRITE "${filters_file}" "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<Project ToolsVersion=\"4.0\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">\n")

  # Gather unique directories for the filters.
  set(filters)

  foreach(f IN LISTS GEN_SOURCES GEN_HEADERS GEN_TESTS)
    get_filename_component(f_dir "${f}" PATH)
    if(NOT f_dir STREQUAL "")
      string(REPLACE "/" ";" dir_parts "${f_dir}")
      set(current_path "")
      foreach(part IN LISTS dir_parts)
        if(current_path STREQUAL "")
          set(current_path "${part}")
        else()
          set(current_path "${current_path}\\${part}")
        endif()
        list(APPEND filters "${current_path}")
      endforeach()
    endif()
  endforeach()

  list(REMOVE_DUPLICATES filters)
  list(SORT filters)

  if(filters)
    file(APPEND "${filters_file}" "  <ItemGroup>\n")
    foreach(f IN LISTS filters)
      execute_process(
        COMMAND uuidgen
        OUTPUT_VARIABLE GENERATED_GUID
        OUTPUT_STRIP_TRAILING_WHITESPACE
      )

      file(APPEND "${filters_file}" "    <Filter Include=\"${f}\">\n")
      file(APPEND "${filters_file}" "      <UniqueIdentifier>{${GENERATED_GUID}}</UniqueIdentifier>\n")
      file(APPEND "${filters_file}" "    </Filter>\n")
    endforeach()
    file(APPEND "${filters_file}" "  </ItemGroup>\n")
  endif()

  if(GEN_SOURCES)
    file(APPEND "${filters_file}" "  <ItemGroup>\n")
    foreach(f IN LISTS GEN_SOURCES)
      get_filename_component(f_dir "${f}" PATH)
      string(REPLACE "/" "\\" f_dir "${f_dir}")
      file(APPEND "${filters_file}" "    <ClCompile Include=\"${f}\">\n")
      file(APPEND "${filters_file}" "      <Filter>${f_dir}</Filter>\n")
      file(APPEND "${filters_file}" "    </ClCompile>\n")
    endforeach()
    file(APPEND "${filters_file}" "  </ItemGroup>\n")
  endif()

  if(GEN_HEADERS)
    file(APPEND "${filters_file}" "  <ItemGroup>\n")
    foreach(f IN LISTS GEN_HEADERS)
      get_filename_component(f_dir "${f}" PATH)
      string(REPLACE "/" "\\" f_dir "${f_dir}")
      file(APPEND "${filters_file}" "    <ClInclude Include=\"${f}\">\n")
      file(APPEND "${filters_file}" "      <Filter>${f_dir}</Filter>\n")
      file(APPEND "${filters_file}" "    </ClInclude>\n")
    endforeach()
    file(APPEND "${filters_file}" "  </ItemGroup>\n")
  endif()

  if(GEN_TESTS)
    file(APPEND "${filters_file}" "  <ItemGroup>\n")
    foreach(f IN LISTS GEN_TESTS)
      get_filename_component(f_dir "${f}" PATH)
      string(REPLACE "/" "\\" f_dir "${f_dir}")
      file(APPEND "${filters_file}" "    <ClCompile Include=\"${f}\">\n")
      file(APPEND "${filters_file}" "      <Filter>${f_dir}</Filter>\n")
      file(APPEND "${filters_file}" "    </ClCompile>\n")
    endforeach()
    file(APPEND "${filters_file}" "  </ItemGroup>\n")
  endif()

  file(APPEND "${filters_file}" "</Project>\n")
  set(${target_name}_FILTERS "${filters_file}" PARENT_SCOPE)

  # The files must be registered in a .props file in order for them to show up in the solution.
  message(STATUS "Generating .props for target: ${target_name}")

  set(props_file "${CMAKE_CURRENT_SOURCE_DIR}/${target_name}.props")
  file(WRITE "${props_file}" "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<Project xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">\n")

  if(GEN_INCLUDES)
    file(APPEND "${props_file}" "  <PropertyGroup>\n")
    file(APPEND "${props_file}" "    <IncludePath>")
    foreach(dir IN LISTS GEN_INCLUDES)
      file(APPEND "${props_file}" "${dir};")
    endforeach()
    file(APPEND "${props_file}" "%(IncludePath)")
    file(APPEND "${props_file}" "</IncludePath>\n")
    file(APPEND "${props_file}" "  </PropertyGroup>\n")
  endif()

  if(GEN_SOURCES)
    file(APPEND "${props_file}" "  <ItemGroup>\n")
    foreach(f IN LISTS GEN_SOURCES)
      get_filename_component(f_dir "${f}" PATH)
      file(APPEND "${props_file}" "    <ClCompile Include=\"${f}\" />\n")
    endforeach()
    file(APPEND "${props_file}" "  </ItemGroup>\n")
  endif()

  if(GEN_HEADERS)
    file(APPEND "${props_file}" "  <ItemGroup>\n")
    foreach(f IN LISTS GEN_HEADERS)
      get_filename_component(f_dir "${f}" PATH)
      file(APPEND "${props_file}" "    <ClInclude Include=\"${f}\" />\n")
    endforeach()
    file(APPEND "${props_file}" "  </ItemGroup>\n")
  endif()

  if(GEN_TESTS)
    file(APPEND "${props_file}" "  <ItemGroup>\n")
    foreach(f IN LISTS GEN_TESTS)
      get_filename_component(f_dir "${f}" PATH)
      file(APPEND "${props_file}" "    <ClCompile Include=\"${f}\" />\n")
    endforeach()
    file(APPEND "${props_file}" "  </ItemGroup>\n")
  endif()

  file(APPEND "${props_file}" "</Project>\n")
  set(${target_name}_PROPS "${props_file}" PARENT_SCOPE)
endfunction()
