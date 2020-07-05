namespace   MyNamespace.Paxos

open System.Net
open MyNamespace.util

type Acceptor(env, id:string)=
    inherit Node("acceptor", id.ep)
    
    let mutable _bn = BallotNumber(-1L, "0.0.0.0:0")
    let mutable _accepted = List.empty
    member me.bn 
        with get() = _bn
        and  set(value) = _bn <- value 
    member me.accepted         
        with get() = _accepted
        and  set(value) = _accepted <- value 

    override me.body()=
        printf "Here I am Acceptor: %A\r\n" id
        while true do
            let msg = me.NextMessage()
            if (msg :? P1aMessage) then
                let msg=msg :?> P1aMessage
                if  me.bn < msg.bn then
                    me.bn <- msg.bn
                    ()
                me.SendMessage(msg.src, P1bMessage(id, me.bn, me.accepted))
                ()
            elif (msg :? P2aMessage) then
                let msg=msg :?> P2aMessage
                if msg.bn = me.bn then
                    me.accepted <- PValue(msg.bn, msg.sn, msg.cmd) ::me.accepted
                    ()
                me.SendMessage(msg.src, P2bMessage(id, me.bn, msg.sn))
                ()
            else
                ()
        ()


// from utils import PValue
// from process import Process
// from message import P1aMessage, P1bMessage, P2aMessage, P2bMessage

// class Acceptor(Process):
//   def __init__(self, env, id):
//     Process.__init__(self, env, id)
//     self.bn = None
//     self.accepted = set()
//     self.env.addProc(self)

//   def body(self):
//     print "Here I am: ", self.id
//     while True:
//       msg = self.getNextMessage()
//       if isinstance(msg, P1aMessage):
//         if msg.bn > self.bn:
//           self.bn = msg.bn
//         self.sendMessage(msg.src,
//                          P1bMessage(self.id,
//                                     self.bn,
//                                     self.accepted))
//       elif isinstance(msg, P2aMessage):
//         if msg.bn == self.bn:
//           self.accepted.add(PValue(msg.bn,
//                                    msg.sn,
//                                    msg.cmd))
//         self.sendMessage(msg.src,
//                          P2bMessage(self.id,
//                                     self.bn,
//                                     msg.sn))