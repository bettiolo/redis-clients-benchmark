Redis Clients Benchmark for .Net Framework
==========================================

The RPUSH benchmark consists of:
- DEL test-queue
- 1000000x RPUSH test-queue json
- LLEN test-queue
- DEL test-queue

Preliminary ASYNC results:

Benchmarking: BookSleve
- benchmark-test-queue-1: 1000000 items
- benchmark-test-queue-1: cleaned up
- Elapsed: 00:00:18.2265814

Benchmarking: RedisBoost
- benchmark-test-queue-3: 1000000 items
- benchmark-test-queue-3: cleaned up
- Elapsed: 00:00:18.6619043

Benchmarking: CsRedis
- benchmark-test-queue-0: 1000000 items
- benchmark-test-queue-0: cleaned up
- Elapsed: 00:00:26.3289267

Benchmarking: ServiceStack
- benchmark-test-queue-2: 1000000 items
- benchmark-test-queue-2: cleaned up
- Elapsed: 00:01:55.8715490
