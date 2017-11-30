<?php
/**
 * Created by IntelliJ IDEA.
 * User: zhonzh
 * Date: 11/30/2017
 * Time: 2:13 PM
 */

require 'vendor/autoload.php';
Predis\Autoloader::register();

$redis = new Predis\Client([
    'scheme' => 'tls',
    'ssl'    => [/* 'cafile' => 'private.pem', */ 'verify_peer' => true],
    'host'   => 'hostName',
    'port'   => 6380,
    'password' => 'password'
]);


function simpleSetGet($redis){
    $redis->set("foo", "bar");
    $value = $redis->get("foo");
    echo $value;
}
