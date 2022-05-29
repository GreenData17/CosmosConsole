using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Command
{
    public string alias;
    public string description;
    public Action<string[]> method;
}
