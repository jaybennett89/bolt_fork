#include "timers.h"

#ifdef UDPKIT_ANDROID
EXPORT_API U32 GetPrecisionTime()
{
    timespec spec = timespec();
    clock_gettime(CLOCK_MONOTONIC, &spec);

    return (spec.tv_sec * 1000) + (spec.tv_nsec / 1000 / 1000);
}
#elif UDPKIT_IOS
double mach_clock()
{
    static bool init = 0;
    static mach_timebase_info_data_t tbInfo;
    static double conversionFactor;
    
    if (!init)
    {
        init = 1 ;
        mach_timebase_info(&tbInfo);
        conversionFactor = tbInfo.numer / (1e9 * tbInfo.denom);
    }
    
    return mach_absolute_time() * conversionFactor;
}

double mach_clock_diff()
{
    static double lastTime = 0;
    double currentTime = mach_clock();
    double diff = currentTime - lastTime;
    lastTime = currentTime;
    return diff;
}

EXPORT_API U32 GetPrecisionTime()
{
    return (U32) (mach_clock() * 1000.0);
}
#endif