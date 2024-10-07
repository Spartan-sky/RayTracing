cmake_minimum_required(VERSION 3.15...3.50)
project(Glad)

add_library(glad ${CMAKE_CURRENT_LIST_DIR}/include/glad/glad.h src/glad.c)
target_include_directories(glad PUBLIC ${CMAKE_CURRENT_LIST_DIR}/include/)