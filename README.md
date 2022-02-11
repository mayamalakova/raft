This is the repo for the Raft algorythm ...

http://thesecretlivesofdata.com/raft/

https://raft.github.io/

https://www.youtube.com/watch?v=6bBggO6KN_k&feature=youtu.be

The paper - https://www.usenix.org/system/files/conference/atc14/atc14-paper-ongaro.pdf

Edge cases:
- what happens with uncommitted log entries - e.g when a leader crashes before it gets confirmation for an update by majority
https://stackoverflow.com/questions/34672331/what-will-happen-to-replicated-but-uncommited-logs-in-raft-protocol

raft is used for replicating Kubernetes' masters cluster store in a key value store called etcd, it is written in Go - https://github.com/etcd-io/etcd/tree/master/raft
