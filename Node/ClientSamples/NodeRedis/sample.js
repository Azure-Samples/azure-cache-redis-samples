'use strict';

var client = require("./connection-helper.js")

var key = 'foo';
var value = 'bar';

var simple_set_get = function () {
    client.set(key, value);
    client.get(key, function (err, reply) {
        if (err) throw err;
        console.log(reply.toString());
    });
};
