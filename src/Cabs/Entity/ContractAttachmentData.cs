﻿using LegacyFighter.Cabs.Common;
using NodaTime;

namespace LegacyFighter.Cabs.Entity;

public class ContractAttachmentData : BaseEntity
{
  public Guid ContractAttachmentNo { get; private set; }
  public byte[] Data { get; private set; }
  public Instant CreationDate { get; private set; } = SystemClock.Instance.GetCurrentInstant();

  public ContractAttachmentData()
  {
  }

  public ContractAttachmentData(Guid contractAttachmentId, byte[] data)
  {
    ContractAttachmentNo = contractAttachmentId;
    Data = data;
  }

  public override bool Equals(object obj)
  {
    if (ReferenceEquals(this, obj)) return true;
    return obj != null && Id != null && Id == (obj as ContractAttachmentData)?.Id;
  }

  public static bool operator ==(ContractAttachmentData left, ContractAttachmentData right)
  {
    return Equals(left, right);
  }

  public static bool operator !=(ContractAttachmentData left, ContractAttachmentData right)
  {
    return !Equals(left, right);
  }
}