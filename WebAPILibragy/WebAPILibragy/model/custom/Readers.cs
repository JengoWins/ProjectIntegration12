﻿namespace WebAPILibragy.model.custom;

[Serializable]
public class Readers
{
    public Readers() { }
    public Readers(string Luser, string Fuser, string patronymic, string email, string phone, string address) 
    {
        last_name = Luser;
        first_name = Fuser;
        this.patronymic = patronymic;
        this.email = email;
        this.phone = phone;
        this.address = address;
    }
    public Readers(database.Readers read) 
    {
        last_name = read.last_name;
        first_name = read.first_name;
        patronymic = read.patronymic;
        email = read.email;
        phone = read.phone;
        address = read.address;
    }
    public string last_name { get; set; }
    public string first_name { get; set; }
    public string patronymic { get; set; }
    public string email { get; set; }
    public string phone { get; set; }
    public string address { get; set; }
}
