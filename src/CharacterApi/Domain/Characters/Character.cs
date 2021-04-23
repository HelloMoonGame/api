﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CharacterApi.Domain.Characters.Rules;
using CharacterApi.Domain.SeedWork;

namespace CharacterApi.Domain.Characters
{
    public class Character : Entity, IAggregateRoot
    {
        public Guid Id { get; }

        public Guid UserId { get; }
        
        [Required]
        public string FirstName { get; }

        [Required]
        public string LastName { get; }

        public SexType Sex { get; }

        private Character() { }
        
        private Character(Guid userId, string firstName, string lastName, SexType sex)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            FirstName = firstName;
            LastName = lastName;
            Sex = sex;

            AddDomainEvent(new CharacterCreatedEvent(Id));
        }

        public static Character Create(
            Guid userId,
            string firstName,
            string lastName,
            SexType sex,
            ISingleCharacterPerUserChecker singleCharacterPerUserChecker)
        {
            CheckRule(new UserCanOnlyHaveOneCharacter(singleCharacterPerUserChecker, userId));

            return new Character(userId, firstName, lastName, sex);
        }
    }
}