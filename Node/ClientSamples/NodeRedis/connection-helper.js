'use strict';

var redis = require('redis');

// change {cacheName} into your own cache name
// var host = '{cacheName}.redis.cache.windows.net'
// change into your password
// var password = 'password'
var port = 6380;

var client = redis.createClient(port, host, {auth_pass: password, tls: {servername: host}});

module.exports = client;

client.on('error', function (err) {
    console.log('Error - ' + err);
});

// Send ping every minute to avoid idle connection been closed
setInterval(function(){
    console.log('redisClient => Sending Ping...');
    client.ping();
}, 1000 * 60);


