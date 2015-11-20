﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace CapdEmulator.Devices
{
  interface IModuleThread
  {
    void Start();
    void Stop();
  }

  abstract class ModuleBase : IModule, IModuleContext
  {
    const int defaultFrequency = 1000;

    bool active;
    IModuleThread thread;

    public int Frequency { get; private set; }
    public ConcurrentQueue<IQuantumDevice> QuantumsQueue { get; private set; }

    public ModuleBase(ModuleType moduleType, ConcurrentQueue<IQuantumDevice> quantumsQueue)
    {
      Id = (byte)moduleType;
      ModuleType = moduleType;
      ChannelCount = 1;
      GainFactor = 0;
      SplineLevel = 0;
      Parameters = new List<IModuleParameter>();

      Frequency = defaultFrequency;
      QuantumsQueue = quantumsQueue;

      active = false;
    }

    protected abstract IModuleThread CreateThread();

    #region IModule

    public byte Id { get; private set; }

    public ModuleType ModuleType { get; private set; }

    public byte ChannelCount { get; private set; }

    public float GainFactor { get; private set; }

    public byte SplineLevel { get; private set; }

    public abstract uint Version { get; protected set; }

    public abstract uint Serial { get; protected set; }

    public abstract string Description { get; protected set; }

    public IList<IModuleParameter> Parameters { get; protected set; }

    public void Execute(Command command, byte[] parameters)
    {
      
    }

    public void SetADCFreq(int frequency)
    {
      Frequency = frequency;
    }

    public void Start()
    {
      if (!active)
      {
        thread = CreateThread();
        thread.Start();
        active = true;
      }
    }

    public void Stop()
    {
      if (active)
      {
        thread.Stop();
        active = false;
      }
    }

    #endregion
  }

  class PressModule : ModuleBase
  {
    public PressModule(ConcurrentQueue<IQuantumDevice> quantumsQueue)
      : base(ModuleType.Press, quantumsQueue)
    {
      Version = 5;
      Serial = 1000001;
      Description = "";

      Parameters.Add(new ModuleParameter(1, 24, ModuleParameterDescription.digitCapacityAdc));
      Parameters.Add(new ModuleParameter(2, 10, ModuleParameterDescription.levelIonAdc));
      Parameters.Add(new ModuleParameter(11, 132.00047302, ModuleParameterDescription.channelPulse));
      Parameters.Add(new ModuleParameter(12, 132.00047302, ModuleParameterDescription.channelPulse));
      Parameters.Add(new ModuleParameter(51, 0.00079999997979, ModuleParameterDescription.channelPress));
      Parameters.Add(new ModuleParameter(60, 1, ModuleParameterDescription.typePress));
      Parameters.Add(new ModuleParameter(70, 2232, ModuleParameterDescription.frequency));
    }

    #region ModuleBase

    public override uint Version { get; protected set; }

    public override uint Serial { get; protected set; }

    public override string Description { get; protected set; }

    protected override IModuleThread CreateThread()
    {
      return new ModuleThread(this, new PressNullGenerator(Frequency));
    }

    #endregion
  }

  class PulseModule : ModuleBase
  {
    public PulseModule(ConcurrentQueue<IQuantumDevice> quantumsQueue)
      : base(ModuleType.Pulse, quantumsQueue)
    {
      Version = 3;
      Serial = 1000002;
      Description = "";

      Parameters.Add(new ModuleParameter(1, 22, ModuleParameterDescription.digitCapacityAdc));
      Parameters.Add(new ModuleParameter(2, 5, ModuleParameterDescription.levelIonAdc));
      Parameters.Add(new ModuleParameter(11, 149.0191803, ModuleParameterDescription.channelPulse));      
      Parameters.Add(new ModuleParameter(51, 8, ModuleParameterDescription.digitCapacityDac));
      //Parameters.Add(new ModuleParameter(51, 0, ModuleParameterDescription.digitCapacityDac)); // Нет аппаратного интегратора.
      Parameters.Add(new ModuleParameter(52, 5, ModuleParameterDescription.levelIonDac));
      Parameters.Add(new ModuleParameter(53, 10.1, ModuleParameterDescription.dacToAdc));
      Parameters.Add(new ModuleParameter(70, 2232, ModuleParameterDescription.frequency));
    }

    #region ModuleBase

    public override uint Version { get; protected set; }

    public override uint Serial { get; protected set; }

    public override string Description { get; protected set; }

    protected override IModuleThread CreateThread()
    {
      return new ModuleThread(this, new PulseSinusGenerator(Frequency));
    }

    #endregion
  }

  class NullModule : ModuleBase
  {
    class NullModuleThread : IModuleThread
    {
      public void Start() { }
      public void Stop() { }
    }

    public NullModule() : base(ModuleType.Null, null) 
    {
      Version = 1;
      Serial = 1;
      Description = "Нулевой модуль";
    }

    #region ModuleBase

    public override uint Version { get; protected set; }

    public override uint Serial  { get; protected set; }

    public override string Description { get; protected set; }

    protected override IModuleThread CreateThread()
    {
      return new NullModuleThread();
    }

    #endregion
  }
}
