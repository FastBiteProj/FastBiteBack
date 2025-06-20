﻿namespace FastBite.Core.Interfaces;

public interface IBlackListService
{
    public bool IsTokenBlackListed(string token);
    public void AddTokenToBlackList(string token);
}
