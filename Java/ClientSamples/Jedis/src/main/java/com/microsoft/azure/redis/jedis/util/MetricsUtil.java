package com.microsoft.azure.redis.jedis.util;

import com.sun.management.OperatingSystemMXBean;

import java.lang.management.ManagementFactory;
import java.text.DecimalFormat;

public class MetricsUtil {
    public static String getSystemMetrics() {
        OperatingSystemMXBean operatingSystemMXBean = (OperatingSystemMXBean) ManagementFactory.getOperatingSystemMXBean();
        String freeMemorySize = toReadableSize(Runtime.getRuntime().freeMemory());
        String maxMemorySize = toReadableSize(Runtime.getRuntime().maxMemory());
        String totalMemorySize = toReadableSize(Runtime.getRuntime().totalMemory());
        String systemCpuLoad = toReadablePercent(operatingSystemMXBean.getSystemCpuLoad());
        String processCpuLoad = toReadablePercent(operatingSystemMXBean.getProcessCpuLoad());

        return String.format("Free memory: %s, Max memory: %s, Total memory: %s, System CPU load: %s, Process CPU load: %s", freeMemorySize, maxMemorySize, totalMemorySize, systemCpuLoad, processCpuLoad);
    }

    private static String toReadableSize(long size) {
        if(size <= 0) return "0";
        final String[] units = new String[] { "B", "kB", "MB", "GB", "TB" };
        int digitGroups = (int) (Math.log10(size)/Math.log10(1024));
        return new DecimalFormat("#,##0.#").format(size/Math.pow(1024, digitGroups)) + " " + units[digitGroups];
    }

    private static String toReadablePercent(double percent){
        return String.format("%.02f %%", percent * 100);
    }

}
