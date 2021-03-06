﻿using ILeaf.Core.Models;
using ILeaf.Repository;
using System.Collections.Generic;
using System;
using System.Linq;
using ILeaf.Core;
using ILeaf.Core.Enums;

namespace ILeaf.Service
{
    public interface IAppointmentService : IBaseService<Appointment>
    {
        List<Appointment> GetPersonalAppointments(long userId);
        List<Appointment> GetGroupAppointments(long groupId);
        List<Appointment> GetFriendAppointment(long userId);
        void SendAppointmentInvition(Appointment appointment, long receiverUserID);
        List<AppointmentShareToUser> GetAllAppointmentInvition(long userId);
        void AcceptAppointmentInvition(long appointmentId);
        void DeclineAppointmentInvition(long appointmentId);
        void SendAppointmentToGroup(Appointment appointment, long groupId);
        List<Appointment> ShowAppointmentsToCurrentUser(long userId);
    }

    public class AppointmentService : BaseService<Appointment>, IAppointmentService
    {
        private IAppointmentShareToUserRepository share_repo = StructureMap.ObjectFactory.GetInstance<IAppointmentShareToUserRepository>();

        public AppointmentService(IAppointmentRepository repo) : base(repo)
        {
        }

        /// <summary>
        /// 接受日程邀请
        /// </summary>
        /// <param name="appointmentId"></param>
        /// <param name="senderUserID"></param>
        /// <param name="receiverUserID"></param>
        public void AcceptAppointmentInvition(long appointmentId)
        {
            Account currentUser = Server.HttpContext.Session["Account"] as Account;
            AppointmentShareToUser share = share_repo.GetFirstOrDefaultObject(s => s.AppointmentId == appointmentId && s.UserId==currentUser.Id && !s.IsAccepted);
            if (share == null)
                throw new Exception("日程请求信息不存在");
            share.IsAccepted = true;
            share_repo.Save(share);

            // TODO: 向用户发送消息
        }

        /// <summary>
        /// 拒绝日程邀请
        /// </summary>
        /// <param name="appointmentId"></param>
        /// <param name="senderUserID"></param>
        /// <param name="receiverUserID"></param>
        public void DeclineAppointmentInvition(long appointmentId)
        {
            Account currentUser = Server.HttpContext.Session["Account"] as Account;
            AppointmentShareToUser share = share_repo.GetFirstOrDefaultObject(s => s.AppointmentId == appointmentId && s.UserId == currentUser.Id && !s.IsAccepted);
            if (share == null)
                throw new Exception("日程请求信息不存在");
            share_repo.Delete(share);

            // TODO: 向用户发送消息
        }

        /// <summary>
        /// 获取所有日程邀请
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<AppointmentShareToUser> GetAllAppointmentInvition(long userId)
        {
            var share = share_repo.GetObjectList(s => s.UserId == userId
               && !s.IsAccepted, s => s.AppointmentId, Core.Enums.OrderingType.Descending, 0, 0);
            return share;
        }

        /// <summary>
        /// 获取在用户主页上展示的日程
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<Appointment> ShowAppointmentsToCurrentUser(long userId)
        {
            Account currentUser = Server.HttpContext.Session["Account"] as Account;
            Account thatUser = StructureMap.ObjectFactory.GetInstance<IAccountService>().GetObject(userId);

            if (thatUser == null)
                throw new Exception("用户信息不存在！");

            if (currentUser == null)
                return thatUser.Appointments.Where(a => a.Visibily == 0).ToList();

            IFriendshipRepository fr = StructureMap.ObjectFactory.GetInstance<IFriendshipRepository>();
            IGroupRepository gr = StructureMap.ObjectFactory.GetInstance<IGroupRepository>();
            bool isFriend = fr.GetFirstOrDefaultObject(f => f.IsAccepted && ((f.Account1 == userId && f.Account2 == currentUser.Id)
                || (f.Account1 == currentUser.Id && f.Account2 == userId))) != null;
            bool isClassmate = currentUser.ClassId == thatUser.ClassId;
            bool isGroupmate = (from g in currentUser.BelongtoGroups.ToList().ConvertAll(x => x.GroupId)
                                where thatUser.BelongtoGroups.ToList().ConvertAll(x => x.GroupId).Contains(g)
                                select g).Count() != 0;

            List<Appointment> appointments = new List<Appointment>();

            appointments = appointments.Union(thatUser.Appointments.Where(a => a.Visibily == 0)).ToList();

            if (isClassmate)
                appointments = appointments.Union(thatUser.Appointments.Where(a => a.Visibily == 1)).ToList();

            if (isGroupmate)
                appointments = appointments.Union(thatUser.Appointments.Where(a => a.Visibily == 2)).ToList();

            if (isFriend)
                appointments = appointments.Union(thatUser.Appointments.Where(a => a.Visibily == 3)).ToList();

            return appointments;
        }

        /// <summary>
        /// 获取已接受邀请的日程
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<Appointment> GetFriendAppointment(long userId)
        {
            var share = share_repo.GetObjectList(s => s.UserId == userId
                && s.IsAccepted, s => s.AppointmentId, Core.Enums.OrderingType.Descending, 0, 0);
            return (from AppointmentShareToUser s in share select s.Appointment).ToList();
        }

        /// <summary>
        /// 获取小组日程
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public List<Appointment> GetGroupAppointments(long groupId)
        {
            IGroupService groupService = StructureMap.ObjectFactory.GetInstance<IGroupService>();
            Group group = groupService.GetObject(groupId);
            if (group == null)
                throw new Exception("小组信息不存在");

            return group.Appointments.ToList().ConvertAll(x => x.Appointment);
        }

        /// <summary>
        /// 获取个人日程
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<Appointment> GetPersonalAppointments(long userId)
        {
            var appointments = GetFullList(a => a.CreatorId == userId, a => a.Id, Core.Enums.OrderingType.Descending);
            return appointments;
        }

        /// <summary>
        /// 发送日程请求
        /// </summary>
        /// <param name="appointmentId"></param>
        /// <param name="receiverUserID"></param>
        public void SendAppointmentInvition(Appointment appointment, long receiverUserID)
        {
            SaveObject(appointment);
            AppointmentShareToUser share = new AppointmentShareToUser()
            {
                AppointmentId = appointment.Id,
                IsAccepted = false,
                UserId = receiverUserID
            };
            share_repo.Save(share);
        }

        /// <summary>
        /// 发送日程到小组
        /// </summary>
        /// <param name="appointmentId"></param>
        /// <param name="groupId"></param>
        public void SendAppointmentToGroup(Appointment appointment, long groupId)
        {
            Account currentUser = Server.HttpContext.Session["Account"] as Account;
            IGroupService groupService = StructureMap.ObjectFactory.GetInstance<IGroupService>();
            Group group = groupService.GetObject(groupId);

            if (appointment == null)
                throw new Exception("日程信息不存在");

            if (group == null)
                throw new Exception("小组信息不存在");

            if (group.HeadmanId != currentUser.Id)
                throw new Exception("只有组长才能添加小组日程");

            IGroupAppointmentRepository groupAppointmentRepository = StructureMap.ObjectFactory.GetInstance<IGroupAppointmentRepository>();
            groupAppointmentRepository.Add(new GroupAppointment()
            {
                Appointments_Id = appointment.Id,
                Groups_Id = groupId
            });
            groupAppointmentRepository.SaveChanges();
        }

        public void SendAppointmentToClass(Appointment appointment,long classId)
        {
            Account currentUser = Server.HttpContext.Session["Account"] as Account;
            IClassInfoRepository classInfoRepository = StructureMap.ObjectFactory.GetInstance<IClassInfoRepository>();
            ClassInfo group = classInfoRepository.GetObjectById(classId);

            if (appointment == null)
                throw new Exception("日程信息不存在");

            if (group == null)
                throw new Exception("班级信息不存在");

            if (group.InstructorId != currentUser.Id)
                throw new Exception("只有该班的辅导员才能添加班级日程");

            IClassAppointmentRepository classAppointmentRepository = StructureMap.ObjectFactory.GetInstance<IClassAppointmentRepository>();
            classAppointmentRepository.Add(new ClassAppointment()
            {
                Appointments_Id = appointment.Id,
                Classes_Id = classId,
            });
            classAppointmentRepository.SaveChanges();
        }

        public override void DeleteObject(Appointment obj)
        {
            Account currentUser = Server.HttpContext.Session["Account"] as Account;

            if (currentUser.Id != obj.CreatorId)
                throw new Exception("只有日程创建者才能删除日程！");
            
            obj.Shares.Clear();
            obj.Groups.Clear();

            base.DeleteObject(obj);
        }
    }
}
