package benchmark;

import java.time.Duration;
import java.util.Date;

public class Interval {
    private Date startTime;
    private Date endTime;
    private Duration duration;

    public Date getStartTime() {
        return startTime;
    }

    public Date getEndTime() {
        return endTime;
    }

    public void start(){
        this.startTime = new Date();
    }

    public void end(){
        this.endTime = new Date();
        this.duration = Duration.ofMillis(endTime.getTime() - startTime.getTime());
    }

    public Duration getDuration(){
        return this.duration;
    }

    @Override
    public String toString() {
        return String.format("[%s, %s, %s]", startTime, endTime, duration);
    }
}
