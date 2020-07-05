namespace MyNamespace.Paxos

open System.Net
open MyNamespace.util

type Scout (env, leader:string, acceptors:string list, bn ) as me =
    inherit Node("scout", new IPEndPoint(leader.ep.Address, 0))

    let id=(new IPEndPoint(leader.ep.Address, 0)).ToString()

    member me.start()= 
        me.thread.Start()

    override me.body()=
        printf "Here I am Scout: %A\r\n" id
        let mutable waitfor = Set.empty
        for a in acceptors do
            me.SendMessage(a, P1aMessage(id, bn))
            waitfor <- Set.add a waitfor

        let mutable pvalues:PValue list = List.empty
        let mutable more=true
        while more do
            let msg = me.NextMessage()
            if (msg :? P1bMessage) then
                let msg = msg :?> P1bMessage
                if bn = msg.bn && Set.contains msg.src waitfor then
                    waitfor <- Set.remove msg.src waitfor                    
                    // pvalues.update(msg.accepted)
                    pvalues <- pvalues @ msg.accepted                    
                    if float(Set.count waitfor) < float(acceptors.Length)/2.0 then
                        me.SendMessage(leader, AdoptedMessage(id, bn, pvalues))
                        more <- false
                else
                    me.SendMessage(leader,PreemptedMessage(id, msg.bn))
                    more <- false 
            else 
                printf "Scout: unexpected msg\r\n"
                more <- false
        ()

// from process import Process
// from message import P1aMessage, P1bMessage, PreemptedMessage, AdoptedMessage

// class Scout(Process):
//   def __init__(self, env, id, leader, acceptors, bn):
//     Process.__init__(self, env, id)
//     self.leader = leader
//     self.acceptors = acceptors
//     self.bn = bn
//     self.env.addProc(self)

//   def body(self):
//     waitfor = set()
//     for a in self.acceptors:
//       self.sendMessage(a, P1aMessage(self.id, self.bn))
//       waitfor.add(a)

//     pvalues = set()
//     while True:
//       msg = self.getNextMessage()
//       if isinstance(msg, P1bMessage):
//         if self.bn == msg.bn and msg.src in waitfor:
//           pvalues.update(msg.accepted)
//           waitfor.remove(msg.src)
//           if len(waitfor) < float(len(self.acceptors))/2:
//             self.sendMessage(self.leader,
//                              AdoptedMessage(self.id,
//                                             self.bn,
//                                             pvalues))
//             return
//         else:
//           self.sendMessage(self.leader,
//                            PreemptedMessage(self.id,
//                                             msg.bn))
//           return
//       else:
//         print "Scout: unexpected msg"