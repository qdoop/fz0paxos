namespace   MyNamespace.Paxos

open System.Net
open MyNamespace.util

type Message(src:string)=
    let mutable _src=src
    member me._kind="Message"
    member me.src
        with get()=_src
        and  set(value)= _src <- value

type P1aMessage(src, bn:BallotNumber)=
    inherit Message(src)
    member me._kind="P1aMessage"
    member me.bn = bn

type P1bMessage(src, bn:BallotNumber, accepted:PValue list)=
    inherit Message(src)
    member me._kind="P1bMessage"
    member me.bn = bn
    member me.accepted = accepted

type P2aMessage(src, bn:BallotNumber, sn:int64, cmd)=
    inherit Message(src)
    member me._kind="P2aMessage"
    member me.bn = bn
    member me.sn = sn
    member me.cmd = cmd

type P2bMessage(src, bn:BallotNumber, sn:int64)=
    inherit Message(src)
    member me._kind="P2bMessage"
    member me.bn = bn
    member me.sn = sn

type PreemptedMessage(src, bn:BallotNumber)=
    inherit Message(src)
    member me._kind="PreemptedMessage"
    member me.bn = bn

type AdoptedMessage(src, bn:BallotNumber, accepted: PValue list)=
    inherit Message(src)
    member me._kind="AdoptedMessage"
    member me.bn = bn
    member me.accepted = accepted

type DecisionMessage(src,sn:int64,cmd)=
    inherit Message(src)
    member me._kind="DecisionMessage"
    member me.sn = sn
    member me.cmd = cmd

type RequestMessage(src, cmd)=
    inherit Message(src)
    member me._kind="RequestMessage"
    member me.cmd = cmd

type ProposeMessage(src, sn:int64, cmd)=
    inherit Message(src)
    member me._kind="ProposeMessage"
    member me.sn = sn
    member me.cmd = cmd




// class Message:
//   def __init__(self, src):
//     self.src = src

//   def __str__(self):
//     return str(self.__dict__)

// class P1aMessage(Message):
//   def __init__(self, src, bn):
//     Message.__init__(self, src)
//     self.bn = bn

// class P1bMessage(Message):
//   def __init__(self, src, bn, accepted):
//     Message.__init__(self, src)
//     self.bn = bn
//     self.accepted = accepted

// class P2aMessage(Message):
//   def __init__(self, src, bn, sn, cmd):
//     Message.__init__(self, src)
//     self.bn = bn
//     self.sn = sn
//     self.cmd = cmd

// class P2bMessage(Message):
//   def __init__(self, src, bn, sn):
//     Message.__init__(self, src)
//     self.bn = bn
//     self.sn = sn

// class PreemptedMessage(Message):
//   def __init__(self, src, bn):
//     Message.__init__(self, src)
//     self.bn = bn

// class AdoptedMessage(Message):
//   def __init__(self, src, bn, accepted):
//     Message.__init__(self, src)
//     self.bn = bn
//     self.accepted = accepted

// class DecisionMessage(= sn
//     self.cmd = cmd

// class RequestMessage(Message):
//   def __init__(self, src, cmd):
//     Message.__init__(self, src)
//     self.cmd = cmd

// class ProposeMessage(Message):
//   def __init__(self, src, sn, cmd):
//     Message.__init__(self, src)
//     self.sn = sn
//     self.cmd = cmd