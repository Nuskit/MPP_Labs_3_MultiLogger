﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labs_3_Logger
{
  public interface IConvertMessage
  {
    string ConvertMessage(LogLevel logLevel, string message);
  }
}
