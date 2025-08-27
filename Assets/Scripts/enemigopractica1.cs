using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy
{
    public string Name;
    protected List<string> _skills;
    protected List<string> _weaknesses;

    public int LifePoints;

    public Enemy(string name, int life)
    {
        Name = name;
        _skills = new List<string>();
        _weaknesses = new List<string>();
        LifePoints = life;
    }
    public virtual void Attack()
    {
        Debug.Log($"{Name} Ataca!!");
    }
    public virtual void AddSkill(string skill)
    {
        _skills.Add(skill);
    }
    public virtual void AddWeak(string weak)
    {
        _weaknesses.Add(weak);
    }
    public virtual void DefeatText()
    {
        Debug.Log("Mi destino ha llego a su final...");
    }
    public void ShowSkills()
    {
        string skillsList = "Habilidades: ";
        if (_skills.Count == 0)
        {
            Debug.Log(" Ninguna.");
        }
        else
        {
            foreach (var skillItem in _skills)
            {
                skillsList += skillItem + ", ";
            }
            Debug.Log(skillsList);
        }
    }
    public void ShowWeaks()
    {
        string weaksList = "Debilidades: ";
        if (_weaknesses.Count == 0) Debug.Log(" Ninguna.");
        if (_weaknesses.Count > 0)
        {
            foreach (var weakItem in _weaknesses)
            {
                weaksList += weakItem + ", ";
            }
            Debug.Log(weaksList);
        }
    }
}

    //Clase derivada Monster, heredada de Enemy
    public class Monster : Enemy
    {
        // Constructor de la clase Monster
        public Monster(string name) : base(name, 20) { }
        public override void DefeatText()
        {
            Debug.Log($"{Name} ROOOOOAAAAR *se muere*");
        }
        public override void Attack()
        {
            Debug.Log($"{Name} ruge y ataca con sus garras afiladas!");
        }
    }
    public class Alien : Enemy
    {
        public Alien(string name) : base(name, 10) { }
        public override void DefeatText()
        {
            Debug.Log($"{Name} Tu raza tiene los días contados...");
        }
        public override void Attack()
        {
            Debug.Log($"{Name} Destruye tu mente con pensamientos intrusivos!");
        }
    }
