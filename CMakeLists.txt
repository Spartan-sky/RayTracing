cmake_minimum_required(VERSION 3.15...3.50)
project(raytracing VERSION 1.0 LANGUAGES CXX)

add_subdirectory(${CMAKE_CURRENT_LIST_DIR}/include/glad)
add_subdirectory(${CMAKE_CURRENT_LIST_DIR}/include/glfw)

add_executable(raytracing main.cpp)

target_include_directories(raytracing PUBLIC ${CMAKE_CURRENT_LIST_DIR}/include)

target_link_libraries(raytracing glad)
target_link_libraries(raytracing glfw)