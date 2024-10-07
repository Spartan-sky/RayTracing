Param(
    [switch]$isReleaseOrDebug
)
If($isReleaseOrDebug -eq $false){
    cmake --build build --config release
    .\build\Release\raytracing.exe > image.ppm
} Else {
    cmake --build build
    .\build\Debug\raytracing.exe > image.ppm
}