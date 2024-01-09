<?php

require '../../vendor/autoload.php';

use Predis\Client;

// Environment variables
$host = getenv('REDIS_HOST');
$password = getenv('REDIS_PASSWORD');

// Client instance connected via TLS
$client = new Client("rediss://{$host}:6380?password={$password}");

// PING command
echo "Command PING\n";
echo "Command response: " . $client->ping() . "\n";

// GET command (non-existing key)
echo "Command GET Message\n";
echo "Command response: " . $client->get('Message') . "\n";

// SET command (set previous requested key)
echo "Command SET Message\n";
echo "Command response: " . $client->set('Message', 'Hello, I was set with Predis!') . "\n";

// Retry GET command and now get previously set value.
echo "Command GET Message\n";
echo "Command response: " . $client->get('Message') . "\n";

// CLIENT LIST command to check all connections to your Redis instance.
echo "Command CLIENT LIST\n";
echo "Command response: \n";
var_dump($client->client('LIST'));
