﻿using ColossalFramework;
using ColossalFramework.Math;
using CSURToolBox.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CSURToolBox.CustomData
{
    public static class CustomNetNode
    {
        public static bool RayCast(ref NetNode node, Segment3 ray, float snapElevation, out float t, out float priority)
        {
            NetInfo info = node.Info;
            // NON-STOCK CODE STARTS
            if (CSUROffset.IsCSUROffset(info.m_netAI.m_info))
            {
                return RayCastNodeMasked(ref node, ray, snapElevation, false, out t, out priority);
            }
            // NON-STOCK CODE ENDS
            if ((node.m_flags & (NetNode.Flags.Middle | NetNode.Flags.Outside)) == NetNode.Flags.None)
            {
                float num = (float)node.m_elevation + info.m_netAI.GetSnapElevation();
                float t2;
                if (info.m_netAI.IsUnderground())
                {
                    t2 = Mathf.Clamp01(Mathf.Abs(snapElevation + num) / 12f);
                }
                else
                {
                    t2 = Mathf.Clamp01(Mathf.Abs(snapElevation - num) / 12f);
                }
                float collisionHalfWidth = Mathf.Max(3f, info.m_netAI.GetCollisionHalfWidth());
                float num2 = Mathf.Lerp(info.GetMinNodeDistance(), collisionHalfWidth, t2);
                if (Segment1.Intersect(ray.a.y, ray.b.y, node.m_position.y, out t))
                {
                    float num3 = Vector3.Distance(ray.Position(t), node.m_position);
                    if (num3 < num2)
                    {
                        priority = Mathf.Max(0f, num3 - collisionHalfWidth);
                        return true;
                    }
                }
            }
            t = 0f;
            priority = 0f;
            return false;
        }
        //to detour move it
        public static bool MoveItRayCastNode(ref NetNode node, Segment3 ray, float snapElevation, out float t, out float priority)
        {
            NetInfo info = node.Info;
            // NON-STOCK CODE STARTS
            if (CSUROffset.IsCSUROffset(info.m_netAI.m_info))
            {
                return RayCastNodeMasked(ref node, ray, snapElevation, false, out t, out priority);
            }
            // NON-STOCK CODE ENDS
            //if ((node.m_flags & (NetNode.Flags.Middle | NetNode.Flags.Outside)) == NetNode.Flags.None)
            //{
                float num = (float)node.m_elevation + info.m_netAI.GetSnapElevation();
                float t2;
                if (info.m_netAI.IsUnderground())
                {
                    t2 = Mathf.Clamp01(Mathf.Abs(snapElevation + num) / 12f);
                }
                else
                {
                    t2 = Mathf.Clamp01(Mathf.Abs(snapElevation - num) / 12f);
                }
                float collisionHalfWidth = Mathf.Max(3f, info.m_netAI.GetCollisionHalfWidth());
                float num2 = Mathf.Lerp(info.GetMinNodeDistance(), collisionHalfWidth, t2);
                if (Segment1.Intersect(ray.a.y, ray.b.y, node.m_position.y, out t))
                {
                    float num3 = Vector3.Distance(ray.Position(t), node.m_position);
                    if (num3 < num2)
                    {
                        priority = Mathf.Max(0f, num3 - collisionHalfWidth);
                        return true;
                    }
                }
            //}
            t = 0f;
            priority = 0f;
            return false;
        }

        public static bool RayCastNodeMasked(ref NetNode node, Segment3 ray, float snapElevation, bool bothSides, out float t, out float priority)
        {
            bool lht = false;
            //if (SimulationManager.instance.m_metaData.m_invertTraffic == SimulationMetaData.MetaBool.True) lht = true;
            NetInfo info = node.Info;
            float num = (float)node.m_elevation + info.m_netAI.GetSnapElevation();
            float t2;
            if (info.m_netAI.IsUnderground())
            {
                t2 = Mathf.Clamp01(Mathf.Abs(snapElevation + num) / 12f);
            }
            else
            {
                t2 = Mathf.Clamp01(Mathf.Abs(snapElevation - num) / 12f);
            }
            float collisionHalfWidth = Mathf.Max(3f, info.m_halfWidth);
            float maskHalfWidth = Mathf.Min(collisionHalfWidth - 1.5f, info.m_pavementWidth);
            float num2 = Mathf.Lerp(info.GetMinNodeDistance(), collisionHalfWidth, t2);
            float num2m = Mathf.Lerp(info.GetMinNodeDistance(), maskHalfWidth, t2);
            float num2delta = Mathf.Lerp(info.GetMinNodeDistance(), collisionHalfWidth - maskHalfWidth, t2);
            if (node.CountSegments() != 0)
            {
                NetManager instance = Singleton<NetManager>.instance;
                NetSegment mysegment = CSUROffset.GetSameInfoSegment(node);
                Vector3 direction = CSUROffset.CheckNodeEq(mysegment.m_startNode, node) ? mysegment.m_startDirection : -mysegment.m_endDirection;
                //Debug.Log(direction);
                if ((mysegment.m_flags & NetSegment.Flags.Invert) != 0) lht = true;
                // normal to the right hand side
                Vector3 normal = new Vector3(direction.z, 0, -direction.x).normalized;
                Vector3 trueNodeCenter = node.m_position + (lht ? -collisionHalfWidth : collisionHalfWidth) * normal;
                //Debug.Log($"num2: {num2}, num2m: {num2m}");
                //Debug.Log($"node: {node.m_position}, center: {trueNodeCenter}");
                if (Segment1.Intersect(ray.a.y, ray.b.y, node.m_position.y, out t))
                {
                    float num3 = Vector3.Distance(ray.Position(t), trueNodeCenter);
                    if (num3 < num2delta)
                    {
                        priority = Mathf.Max(0f, num3 - collisionHalfWidth);
                        return true;
                    }
                }

            }
            else
            {
                if (Segment1.Intersect(ray.a.y, ray.b.y, node.m_position.y, out t))
                {
                    float num3 = Vector3.Distance(ray.Position(t), node.m_position);
                    if (num3 < num2)
                    {
                        priority = Mathf.Max(0f, num3 - collisionHalfWidth);
                        return true;
                    }
                }
            }
            t = 0f;
            priority = 0f;
            return false;
        }

        public static void UpdateBuilding(ref NetNode node, ushort nodeID, BuildingInfo newBuilding, float heightOffset)
        {
            float num = 0f;
            if ((object)newBuilding != null)
            {
                NetInfo info = node.Info;
                if ((object)info != null)
                {
                    num = info.m_netAI.GetNodeBuildingAngle(nodeID, ref node);
                }
            }
            BuildingInfo buildingInfo = null;
            if (node.m_building != 0)
            {
                buildingInfo = Singleton<BuildingManager>.instance.m_buildings.m_buffer[node.m_building].Info;
            }
            if ((object)newBuilding != buildingInfo)
            {
                if (node.m_building != 0)
                {
                    Singleton<BuildingManager>.instance.ReleaseBuilding(node.m_building);
                    node.m_building = 0;
                }
                if ((object)newBuilding != null)
                {
                    Vector3 position = node.m_position;
                    position.y += heightOffset;
                    // NON-STOCK CODE STARTS
                    if (CSUROffset.IsCSUROffset(node.Info))
                    {
                        bool lht = false;
                        if (node.CountSegments() != 0)
                        {
                            float collisionHalfWidth = Mathf.Max(3f, (node.Info.m_halfWidth + node.Info.m_pavementWidth) / 2f);
                            NetSegment mysegment = CSUROffset.GetSameInfoSegment(node);
                            Vector3 direction = CSUROffset.CheckNodeEq(mysegment.m_startNode, node) ? mysegment.m_startDirection : -mysegment.m_endDirection;
                            if ((mysegment.m_flags & NetSegment.Flags.Invert) != 0) lht = true;
                            // normal to the right hand side
                            Vector3 normal = new Vector3(direction.z, 0, -direction.x).normalized;
                            position = position + (lht ? -collisionHalfWidth : collisionHalfWidth) * normal;
                        }
                    }
                    // NON-STOCK CODE ENDS
                    num *= 6.28318548f;
                    if ((object)buildingInfo != null || TestNodeBuilding(nodeID, newBuilding, position, num))
                    {
                        Randomizer randomizer = new Randomizer(nodeID);
                        if (Singleton<BuildingManager>.instance.CreateBuilding(out node.m_building, ref randomizer, newBuilding, position, num, 0, node.m_buildIndex + 1))
                        {
                            Singleton<BuildingManager>.instance.m_buildings.m_buffer[node.m_building].m_flags |= (Building.Flags.Untouchable | Building.Flags.FixedHeight);
                        }
                    }
                }
            }
            else if (node.m_building != 0)
            {
                BuildingManager instance = Singleton<BuildingManager>.instance;
                Vector3 position2 = node.m_position;
                position2.y += heightOffset;
                // NON-STOCK CODE STARTS
                if (CSUROffset.IsCSUROffset(node.Info))
                {
                    bool lht = false;
                    if (node.CountSegments() != 0)
                    {
                        float collisionHalfWidth = Mathf.Max(3f, (node.Info.m_halfWidth + node.Info.m_pavementWidth)/2f);
                        NetSegment mysegment = CSUROffset.GetSameInfoSegment(node);
                        Vector3 direction = CSUROffset.CheckNodeEq(mysegment.m_startNode, node) ? mysegment.m_startDirection : -mysegment.m_endDirection;
                        if ((mysegment.m_flags & NetSegment.Flags.Invert) != 0) lht = true;
                        // normal to the right hand side
                        Vector3 normal = new Vector3(direction.z, 0, -direction.x).normalized;
                        position2 = position2 + (lht ? -collisionHalfWidth : collisionHalfWidth) * normal;
                    }
                }
                // NON-STOCK CODE ENDS
                num *= 6.28318548f;
                // NON-STOCK CODE STARTS
                if (CSUROffset.IsCSUROffset(node.Info) && (instance.m_buildings.m_buffer[node.m_building].m_position != position2 || instance.m_buildings.m_buffer[node.m_building].m_angle != num))
                {
                    instance.m_buildings.m_buffer[node.m_building].m_position = position2;
                    instance.m_buildings.m_buffer[node.m_building].m_angle = num;
                    instance.UpdateBuilding(node.m_building);
                }
                else
                {
                    if (instance.m_buildings.m_buffer[node.m_building].m_position.y != position2.y || instance.m_buildings.m_buffer[node.m_building].m_angle != num)
                    {
                        instance.m_buildings.m_buffer[node.m_building].m_position.y = position2.y;
                        instance.m_buildings.m_buffer[node.m_building].m_angle = num;
                        instance.UpdateBuilding(node.m_building);
                    }
                }
                               
                // NON-STOCK CODE ENDS
            }
        }

        public static bool TestNodeBuilding(ushort nodeID, BuildingInfo info, Vector3 position, float angle)
        {
            Vector2 a = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 a2 = new Vector3(a.y, 0f - a.x);
            if (info.m_placementMode == BuildingInfo.PlacementMode.Roadside || info.m_placementMode == BuildingInfo.PlacementMode.PathsideOrGround)
            {
                a *= (float)info.m_cellWidth * 4f - 0.8f;
                a2 *= (float)info.m_cellLength * 4f - 0.8f;
            }
            else
            {
                a *= (float)info.m_cellWidth * 4f;
                a2 *= (float)info.m_cellLength * 4f;
            }
            if (info.m_circular)
            {
                a *= 0.7f;
                a2 *= 0.7f;
            }
            ItemClass.CollisionType collisionType = info.m_buildingAI.GetCollisionType();
            Vector2 a3 = VectorUtils.XZ(position);
            Quad2 quad = default(Quad2);
            quad.a = a3 - a - a2;
            quad.b = a3 - a + a2;
            quad.c = a3 + a + a2;
            quad.d = a3 + a - a2;
            float minY = Mathf.Min(position.y, Singleton<TerrainManager>.instance.SampleRawHeightSmooth(position));
            float maxY = position.y + info.m_generatedInfo.m_size.y;
            if (collisionType == ItemClass.CollisionType.Elevated)
            {
                minY = position.y + info.m_generatedInfo.m_min.y;
            }
            if (Singleton<NetManager>.instance.OverlapQuad(quad, minY, maxY, collisionType, info.m_class.m_layer, nodeID, 0, 0, null))
            {
                return false;
            }
            if (Singleton<BuildingManager>.instance.OverlapQuad(quad, minY, maxY, collisionType, info.m_class.m_layer, 0, nodeID, 0, null))
            {
                return false;
            }
            return true;
        }
    }
}