import { createSlice } from '@reduxjs/toolkit'
import type { PayloadAction } from '@reduxjs/toolkit'
import type { RootState } from '../store'

import { KnowledgeNode } from '../../types/KnowledgeNode'

export interface TimelineNode{
  parentTimeline: TimelineNode | null,
  timeline: TimelineNode[],
  knowledgeNode: KnowledgeNode,
  firstAccesstime: Date
}

export interface navigationTimelineState{
  masterTimeline: TimelineNode[],
  hereAndNow: TimelineNode,
}