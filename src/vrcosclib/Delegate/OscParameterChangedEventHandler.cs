﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BuildSoft.VRChat.Osc;
public delegate void OscParameterChangedEventHandler<TSender>(TSender sender, ParameterChangedEventArgs e);

