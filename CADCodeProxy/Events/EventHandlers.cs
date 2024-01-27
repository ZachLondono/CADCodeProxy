namespace CADCodeProxy.Events;

public delegate void CADCodeProgressEventHandler(CADCodeProgressEventArgs args);
public delegate void CADCodeInfoEventHandler(CADCodeInfoEventArgs args);
public delegate void CADCodeErrorEventHandler(CADCodeErrorEventArgs args);
public delegate void InfoEventHandler(string message);
public delegate void ErrorEventHandler(ErrorEventArgs args);
