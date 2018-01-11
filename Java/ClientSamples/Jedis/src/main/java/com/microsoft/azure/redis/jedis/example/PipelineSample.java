package com.microsoft.azure.redis.jedis.example;

import redis.clients.jedis.Jedis;
import redis.clients.jedis.Pipeline;
import redis.clients.jedis.Response;

import java.util.Set;

public class PipelineSample {

    public static void pipeline(Jedis jedis){
        Pipeline p = jedis.pipelined();
        p.set("fool", "bar");
        p.zadd("foo", 1, "barowitch");  p.zadd("foo", 0, "barinsky"); p.zadd("foo", 0, "barikoviev");
        Response<String> pipeString = p.get("fool");
        Response<Set<String>> sose = p.zrange("foo", 0, -1);
        p.sync();

        int soseSize = sose.get().size();
        Set<String> setBack = sose.get();
    }
}
