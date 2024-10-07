#version 330 core
out vec4 FragColor;

uniform vec4 ourColor;
uniform vec2 u_resolution;

#define SAMPLES_PER_PIXEL 5
#define PIXEL_SAMPLES_SCALE (1. / SAMPLES_PER_PIXEL)

struct Ray{
    vec3 ori;
    vec3 dir;
};

struct Sphere{
    vec3 c; //center
    float r; //radius
    vec3 n; //normal
    bool frontFacing; //is the normal front facing

    vec3 p; //position of ray hit
    float t; //distance ray traveled until hit
};

struct Interval{
    float tmax;
    float tmin;
};

struct HitRecord{
    vec3 p;
    vec3 n;
    float t;
    bool frontFace;
    Sphere s;
};

// A single iteration of Bob Jenkins' One-At-A-Time hashing algorithm
float hash( float n )
{
    return fract(sin(n)*43758.5453);
}

float noise21( in vec2 x )
{
    vec2 i = floor(x);
    vec2 f = fract(x);

    f = f*f*(3.0-2.0*f);

    float n = i.x + i.y*57.0;

    return mix(mix( hash(n+ 0.0), hash(n+ 1.0),f.x),
               mix( hash(n+57.0), hash(n+58.0),f.x),f.y);
}

float noise31(in vec3 x){
    vec3 i = floor(x);
    vec3 f = fract(x);

    f = f*f*(3.0-2.0*f);

    float n = i.x + i.y*57.0;

    return mix(
                mix(
                        mix( hash(n+ 0.0), hash(n+ 1.0),f.x),
                        mix( hash(n+57.0), hash(n+58.0),f.x),
                        f.y
                      ),
                mix(hash(n+123.0), hash(n+127.0),f.y),
                f.z
               );
}

vec3 noise13(float n){
    return vec3(hash(n), hash(n * 1354), hash(n * 645));
}

vec3 at(Ray r, float t)
{
    return r.ori + t * r.dir;
}

void setSphereFaceNormal(out Sphere s, Ray r){
    s.frontFacing = dot(r.dir, s.n) < 0.;
    s.n = s.frontFacing ? s.n : - s.n;
}

void hitSphere(out Sphere s, Interval i,out Ray r){
    vec3 oc = s.c - r.ori;
    float a = length(r.dir)*length(r.dir);
    float h = dot(r.dir, oc);
    float c = length(oc) * length(oc) - s.r * s.r;
    
    float discriminant = h*h - a*c;
    float sqrtd = sqrt(discriminant);
    float root = (h - sqrtd) / a;

    s.p = at(r, root);
    s.n = (s.p - s.c) / s.r;
    setSphereFaceNormal(s, r);

    if(discriminant < 0.)
        s.t = -1.;
    else
        s.t = root;
}

vec3 unitVector(vec3 r)
{
    return r / length(r);
}

vec3 randomUnitVector(float x){
    while(true){
        vec3 p = 2. * (noise13(x) - .5);
        float pLength = length(p);
        if(1e-160 < pLength || pLength <= 1.)
            return p / sqrt(pLength);
    }
}

vec3 randomOnHemisphere(vec3 n){
    vec3 onUnitSphere = randomUnitVector(1564);
    if(dot(onUnitSphere, n) > 0.)
        return onUnitSphere;
    else
        return -onUnitSphere;
}

vec3 offsetRay(vec3 dir){
    vec3 offset = vec3(hash(224654.) -.5, hash(12687.) - .5, 0.);
    return dir + offset;
}

vec3 rayColor(Ray r){
    Interval interval = Interval(0., 1. / 0.);

    for(int i = 0; i < SAMPLES_PER_PIXEL; i++)
    {
        r.dir = offsetRay(r.dir);

        Sphere s1 = Sphere(vec3(0.,0.,-1.), 0.5, vec3(1.), true, vec3(0.), 0.);

        hitSphere(s1, interval, r);
        if( s1.t > 0. ){
            vec3 N = unitVector(at(r, s1.t) - vec3(0,0,-1));
            return .5 * vec3(N + 1);
        }

        Sphere s2 = Sphere(vec3(0.,-100.5,-1.), 100, vec3(1.), true, vec3(0.), 0.);
        hitSphere(s2, interval, r);
        if( s2.t > 0. ){
            vec3 N = unitVector(at(r, s2.t) - vec3(0.,-100.5,-1.));
            return .5 * vec3(N + 1);
        }

        vec3 unitDir = unitVector(r.dir);
        float a = .5 * (unitDir.y + 1.);

        return (1. - a) * vec3(1.,1.,1.) + a * vec3(.5, .7, 1.);
    }
}

vec3 R(vec2 uv, vec3 p, vec3 l, float z) {
    vec3 f = normalize(l-p),
        r = normalize(cross(vec3(0,1,0), f)),
        u = cross(f,r),
        c = p+f*z,
        i = c + uv.x*r + uv.y*u,
        d = normalize(i-p);
    return d;
}

void main()
{
    vec2 uv = (gl_FragCoord.xy * 2. - u_resolution.xy) / u_resolution.y;

    vec3 camera_origin = vec3(0., 0., -2.5);
    Ray ray = Ray(camera_origin, R(uv, camera_origin, vec3(0.), .7));

    vec3 col = vec3( 0.);

    col += rayColor(ray);

    FragColor = vec4(col, 1.);
}