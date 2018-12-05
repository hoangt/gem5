/*
 * Copyright (c) 2013-2014 ARM Limited
 * All rights reserved.
 *
 * The license below extends only to copyright in the software and shall
 * not be construed as granting a license to any other intellectual
 * property including but not limited to intellectual property relating
 * to a hardware implementation of the functionality of the software
 * licensed hereunder.  You may use the software subject to the license
 * terms below provided that you ensure that this notice is replicated
 * unmodified and in its entirety in all distributions of the software,
 * modified or unmodified, in source code or in binary form.
 *
 * Copyright (c) 2005 The Regents of The University of Michigan
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are
 * met: redistributions of source code must retain the above copyright
 * notice, this list of conditions and the following disclaimer;
 * redistributions in binary form must reproduce the above copyright
 * notice, this list of conditions and the following disclaimer in the
 * documentation and/or other materials provided with the distribution;
 * neither the name of the copyright holders nor the names of its
 * contributors may be used to endorse or promote products derived from
 * this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
 * OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
 * LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
 * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *
 * Authors: Ron Dreslinski
 *          Mitch Hayenga
 */

/**
 * @file
 * Hardware Prefetcher Definition.
 */

#include "mem/cache/prefetch/base.hh"

#include <cassert>

#include "base/intmath.hh"
#include "cpu/base.hh"
#include "mem/cache/base.hh"
#include "params/BasePrefetcher.hh"
#include "sim/system.hh"

void
BasePrefetcher::PrefetchListener::notify(const PacketPtr &pkt)
{
    parent.probeNotify(pkt);
}

BasePrefetcher::BasePrefetcher(const BasePrefetcherParams *p)
    : ClockedObject(p), listeners(), cache(nullptr), blkSize(p->block_size),
      lBlkSize(floorLog2(blkSize)), onMiss(p->on_miss), onRead(p->on_read),
      onWrite(p->on_write), onData(p->on_data), onInst(p->on_inst),
      masterId(p->sys->getMasterId(this)), pageBytes(p->sys->getPageBytes()),
      prefetchOnAccess(p->prefetch_on_access)
{
}

void
BasePrefetcher::setCache(BaseCache *_cache)
{
    assert(!cache);
    cache = _cache;

    // If the cache has a different block size from the system's, save it
    blkSize = cache->getBlockSize();
    lBlkSize = floorLog2(blkSize);
}

void
BasePrefetcher::regStats()
{
    ClockedObject::regStats();

    pfIssued
        .name(name() + ".num_hwpf_issued")
        .desc("number of hwpf issued")
        ;

}

bool
BasePrefetcher::observeAccess(const PacketPtr &pkt) const
{
    Addr addr = pkt->getAddr();
    bool fetch = pkt->req->isInstFetch();
    bool read = pkt->isRead();
    bool inv = pkt->isInvalidate();
    bool is_secure = pkt->isSecure();

    if (pkt->req->isUncacheable()) return false;
    if (fetch && !onInst) return false;
    if (!fetch && !onData) return false;
    if (!fetch && read && !onRead) return false;
    if (!fetch && !read && !onWrite) return false;
    if (!fetch && !read && inv) return false;
    if (pkt->cmd == MemCmd::CleanEvict) return false;

    if (onMiss) {
        return !inCache(addr, is_secure) &&
               !inMissQueue(addr, is_secure);
    }

    return true;
}

bool
BasePrefetcher::inCache(Addr addr, bool is_secure) const
{
    return cache->inCache(addr, is_secure);
}

bool
BasePrefetcher::inMissQueue(Addr addr, bool is_secure) const
{
    return cache->inMissQueue(addr, is_secure);
}

bool
BasePrefetcher::samePage(Addr a, Addr b) const
{
    return roundDown(a, pageBytes) == roundDown(b, pageBytes);
}

Addr
BasePrefetcher::blockAddress(Addr a) const
{
    return a & ~((Addr)blkSize-1);
}

Addr
BasePrefetcher::blockIndex(Addr a) const
{
    return a >> lBlkSize;
}

Addr
BasePrefetcher::pageAddress(Addr a) const
{
    return roundDown(a, pageBytes);
}

Addr
BasePrefetcher::pageOffset(Addr a) const
{
    return a & (pageBytes - 1);
}

Addr
BasePrefetcher::pageIthBlockAddress(Addr page, uint32_t blockIndex) const
{
    return page + (blockIndex << lBlkSize);
}

void
BasePrefetcher::probeNotify(const PacketPtr &pkt)
{
    // Don't notify prefetcher on SWPrefetch, cache maintenance
    // operations or for writes that we are coaslescing.
    if (pkt->cmd.isSWPrefetch()) return;
    if (pkt->req->isCacheMaintenance()) return;
    if (pkt->isWrite() && cache != nullptr && cache->coalesce()) return;
    notify(pkt);
}

void
BasePrefetcher::regProbeListeners()
{
    /**
     * If no probes were added by the configuration scripts, connect to the
     * parent cache using the probe "Miss". Also connect to "Hit", if the
     * cache is configured to prefetch on accesses.
     */
    if (listeners.empty() && cache != nullptr) {
        ProbeManager *pm(cache->getProbeManager());
        listeners.push_back(new PrefetchListener(*this, pm, "Miss"));
        if (prefetchOnAccess) {
            listeners.push_back(new PrefetchListener(*this, pm, "Hit"));
        }
    }
}

void
BasePrefetcher::addEventProbe(SimObject *obj, const char *name)
{
    ProbeManager *pm(obj->getProbeManager());
    listeners.push_back(new PrefetchListener(*this, pm, name));
}
