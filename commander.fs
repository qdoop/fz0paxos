namespace MyNamespace.Paxos

open System.Net
open MyNamespace.util



type Commander(env,  leader:string, config:Config, bn:BallotNumber, sn, cmd)as me=
    inherit Node("cmndr", new IPEndPoint(leader.ep.Address, 0))    

    let id=(new IPEndPoint(leader.ep.Address, 0)).ToString()

    member me.start()=
        me.thread.Start()
    override me.body()=
        printfn "Here I am Cmndr: %A" id
        let mutable waitfor = Set.empty
        for a in config.acceptors do
            me.SendMessage(a, P2aMessage(id, bn, sn, cmd))
            waitfor <- Set.add a waitfor

        let mutable more=true
        while more do
            let msg = me.NextMessage()
            if (msg :? P2bMessage) then
                let msg=msg :?> P2bMessage
                if bn = msg.bn && Set.contains msg.src waitfor then
                    waitfor <- Set.remove msg.src waitfor
                    if float(Set.count waitfor) < float(config.acceptors.Length)/2.0 then
                        for r in config.replicas do
                            me.SendMessage(r, DecisionMessage(id, sn, cmd))
                    more <- false
                else
                    me.SendMessage(leader, PreemptedMessage(id, bn))
                    more <- false


// from message import P2aMessage, P2bMessage, PreemptedMessage, DecisionMessage
// from process import Process
// from utils import cmd

// class cmder(Process):
//   def __init__(self, env, id, leader, acceptors, replicas,
//                bn, sn, cmd):
//     Process.__init__(self, env, id)
//     self.leader = leader
//     self.acceptors = acceptors
//     self.replicas = replicas
//     self.bn = bn
//     self.sn = sn
//     self.cmd = cmd
//     self.env.addProc(self)

//   def body(self):
//     waitfor = set()
//     for a in self.acceptors:
//       self.sendMessage(a, P2aMessage(self.id, self.bn,
//                                      self.sn, self.cmd))
//       waitfor.add(a)

//     while True:
//       msg = self.getNextMessage()
//       if isinstance(msg, P2bMessage):
//         if self.bn == msg.bn and msg.src in waitfor:
//           waitfor.remove(msg.src)
//           if len(waitfor) < float(len(self.acceptors))/2:
//             for r in self.replicas:
//               self.sendMessage(r, DecisionMessage(self.id,
//                                                   self.sn,
//                                                   self.cmd))
//             return
//         else:
//           self.sendMessage(self.leader, PreemptedMessage(self.id, msg.bn))
//           return