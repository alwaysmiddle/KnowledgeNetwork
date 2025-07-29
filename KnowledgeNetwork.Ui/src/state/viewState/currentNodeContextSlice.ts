import { createSlice } from '@reduxjs/toolkit'
import type { PayloadAction } from '@reduxjs/toolkit'
import type { RootState } from '../store'

import { KnowledgeNode } from '../../types/KnowledgeNode'

export interface CurrentNodeContextState{
  upperLayer: KnowledgeNode[] | null,
  currentNode: KnowledgeNode | null,
  lowerLayer: KnowledgeNode[] | null,
}

const initialState: CurrentNodeContextState = {
  upperLayer: [],
  currentNode: null,
  lowerLayer: []
}

const counterSlice = createSlice({
  name: "counter",
  initialState,
  reducers: {
    gotoParentNode: (state) => {
      
    },
    decrement: (state) => {
      state.value -= 1
    },
    incrementByAmount: (state, action: PayloadAction<number>) => {
      state.value += action.payload
    },
  },
})
