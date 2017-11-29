'use strict';

var redis = require('redis');

// change {cacheName} into your own cache name
// var host = '{cacheName}.redis.cache.windows.net'
// change into your password
// var password = 'password'
var port = 6380;

var client = redis.createClient(port, host, {auth_pass: password, tls: {servername: host}});

client.on('error', function (err) {
    console.log(new Date().toLocaleString() + ' Error - ' + err);
});

client.on('ready', function () {
    console.log(new Date().toLocaleString() + ' Connection is established')
});

client.on('connect', function () {
   console.log(new Date().toLocaleString() + ' Stream is connected to the server')
});

client.on('reconnecting', function () {
    console.log(new Date().toLocaleString() + ' Try to reconnect to server')
})

var key = 'foo';
var value = 'bar';

setInterval(function () {
    console.log(new Date().toLocaleString())
    client.set(key, value, redis.print);
    client.get(key, function (err, reply) {
        if (err) {
            console.log(err)
        } else {
            console.log(reply.toString());
        }
    });
}, 1000 * 60 * 11);

