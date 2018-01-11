using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CashDesk.Model;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace CashDesk
{
    public class DataAccess : IDataAccess
    {

        private DataContext db;

        public void InitializeDatabaseAsync()
        {
            if (db == null)
            {
                db = new DataContext();
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public int AddMemberAsync(string firstName, string lastName, DateTime birthday)
        {
            checkInit();
            if (firstName == null || lastName == null)
            {
                throw new ArgumentException();
            }
            else
            {
                var duplicateLastName = db.Members.Where(p => p.LastName.ToUpper().Equals(lastName.ToUpper())).ToArray();

                if (duplicateLastName.Count() == 0)
                {
                    db.Members.Add(new Member { FirstName = firstName, LastName = lastName, Birthday = birthday });
                    db.SaveChanges();
                    var newmember = db.Members.Where(p => p.LastName.ToLower().Equals(lastName.ToLower())).ToArray().First();
                    return newmember.MemberNumber;
                }
                else
                {
                    throw new DuplicateNameException();
                }
            }
        }

        public void DeleteMemberAsync(int memberNumber)
        {
            checkInit();
            if (memberNumber < 0)
            {
                throw new ArgumentException();
            }
            else
            {
                var member = db.Members.Where(p => p.MemberNumber.Equals(memberNumber)).First();
                db.Members.Remove(member);
            }

        }

        public IMembership JoinMemberAsync(int memberNumber)
        {
            checkInit();
            if (memberNumber < 0)
            {
                throw new ArgumentException();
            }
            else
            {
                var member = db.Members.Where(p => p.MemberNumber.Equals(memberNumber)).First();

                if (db.Memberships.Where(p => p.End == null && p.Member.Equals(member)).Count() > 0)
                {
                    throw new AlreadyMemberException();
                }
                else
                {
                    var membership = db.Memberships.Add(new Membership { Member = member, Begin = DateTime.Now });
                    db.SaveChanges();

                    return membership.Entity;
                }
            }
        }

        public async Task<IMembership> CancelMembershipAsync(int memberNumber)
        {
            checkInit();
            if (memberNumber < 0)
            {
                throw new ArgumentException();
            }
            else
            {

                var member = await db.Members.Where(p => p.MemberNumber.Equals(memberNumber)).FirstAsync();

                if (db.Memberships.Where(p => p.End == null && p.Member.Equals(member)).Count() == 0)
                {
                    throw new NoMemberException();
                }

                var membership = await db.Memberships.Where(p => p.End == null && p.Member.Equals(member)).FirstAsync();
                membership.End = DateTime.Now;
                await db.SaveChangesAsync();

                return membership;
            }
        }

        public async Task DepositAsync(int memberNumber, decimal amount)
        {
            checkInit();
            if (memberNumber < 0 || amount < 0)
            {
                throw new ArgumentException();
            }

            var member = await db.Members.Where(p => p.MemberNumber.Equals(memberNumber)).FirstAsync();
            var membership = await db.Memberships.Where(p => p.End == null && p.Member.Equals(member)).FirstOrDefaultAsync();

            if (membership == null)
            {
                throw new NoMemberException();
            }

            var deposit = db.Deposits.Add(new Deposit { Membership = membership, Amount = amount });
            db.SaveChanges();

        }

        public async Task<IEnumerable<IDepositStatistics>> GetDepositStatisticsAsync()
        {
            checkInit();

            var deposit = await db.Deposits.GroupBy(p => new { Year = p.Membership.Begin.Year, Member = p.Membership.Member }).Select(p => new DepositStatistics { Member = p.Key.Member, Year = p.Key.Year, TotalAmount = p.Sum(t => t.Amount) }).ToListAsync();
            return deposit;
        }

        public void Dispose()
        {

        }

        public void checkInit()
        {
            if (db == null)
            {
                throw new InvalidOperationException();
            }
        }

    }
}
